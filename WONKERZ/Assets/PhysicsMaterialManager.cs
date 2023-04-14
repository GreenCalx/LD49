using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PhysicsMaterialManager : MonoBehaviour
{
    public Camera cam;
    public static PhysicsMaterialManagerData instance_priv;
    public static PhysicsMaterialManagerData instance
    {
        get
        {
            return instance_priv ??= new PhysicsMaterialManagerData();
        }
    }

    public class PhysicsMaterialManagerData
    {
        [System.Serializable]
        public class SchGround
        {
            public enum EType
            {
                ROAD,
                DESERT,
                WATER,
                AIR,
                NONE,
            };

            public bool needUpdateMesh = false;

            public Vector2 friction;
            public EType type;
            public Vector3 depthPerturbation;
            public Texture2D groundMarks;
            public Texture2D slipMarks;
            public ParticleSystem emitter;

            // each skid patch is a quad
            public const int max_skid_patches = 4096;
            public const float min_dist = 0.5f;
            public const float min_dist_sqr = min_dist * min_dist;
            public const int dataCount = 8 * max_skid_patches;
            public const int triCount = 36 * max_skid_patches;
            public const float decalDepth = -2;

            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Color[] colors;
            public int[] triangles;
            public Vector2[] uvs;

            public Matrix4x4[] objectToWorld;
            public Mesh cube;
            public Vector3 meshTopCorner = new Vector3(0.5f, 0.5f, 0.5f);
            public Vector3 meshBotCorner = new Vector3(-0.5f, -0.5f, -0.5f);
            public Vector3 meshCenter = Vector3.zero;
            public const int maxDecals = 1023;
            public CommandBuffer cmd;

            public int vIdx;
            public int tIdx;

            public Mesh groundMarksMesh;
            public Material groundMarksMaterial;
            public Mesh slipMarksMesh;

            public void AddMarkDecal(in Vector3 position, in Matrix4x4 basis, in float width, in Color color, ref int lastIdx)
            {
                var lastMatrix = objectToWorld[lastIdx];
                if (lastMatrix.determinant == 0) lastMatrix = Matrix4x4.identity;

                var normal = basis.GetColumn(0);
                var forward = basis.GetColumn(1);
                var right = basis.GetColumn(2);
                var halfWidth = width * 0.5f;
                Vector4 position4 = new Vector4(position.x, position.y, position.z, 1);
                Vector3 center = (position4 + (halfWidth * right) + (position4 - (halfWidth * right) - (min_dist * forward) + (decalDepth * normal))) * 0.5f;
                Vector3 lastCenter = lastMatrix * meshCenter;
                var dir = center - lastCenter;
                var dist = dir.sqrMagnitude;

                // cull if not far enough yet
                //if (dist == 1 || dist < min_dist_sqr)
                //    return;

                Vector3 scale = new Vector3(width, -decalDepth, dir.magnitude);
                if (dist > min_dist_sqr * 10)
                {
                    scale.z = min_dist;
                    center = (position4 + (halfWidth * right) + (position4 - (halfWidth * right) - (min_dist * forward) + (decalDepth * normal))) * 0.5f;
                    //position4 = new Vector3(0, 16, 0) + new Vector3(0f, 2.5f, 2.5f);
                    //center = ((position4 + (2.5f * right)) + (position4 - (2.5f * right) - (5f * forward) + (-5f * normal))) / 2f;
                    //scale = new Vector3(5, 5, 5);
                }
                else
                {
                    //basis = Schnibble.SchMathf.GetBasis(basis.GetColumn(0), dir.normalized);
                    scale.z = dir.magnitude;
                }

                Matrix4x4 basisUnity = new Matrix4x4(basis.GetColumn(2), basis.GetColumn(0), basis.GetColumn(1), basis.GetColumn(3));

                lastIdx = (++vIdx) % maxDecals;
                objectToWorld[lastIdx].SetTRS(center, basisUnity.rotation, scale);

                needUpdateMesh = true;
            }

            public int NextIdx()
            {
                vIdx = (++vIdx) % dataCount;
                return vIdx;
            }

            public int NextTriIdx()
            {
                tIdx = (++tIdx) % triCount;
                return tIdx;
            }

            //   (0, y+1)  7|--------\8   (1, y+1)
            //      (0, y)  |\2------1\   (1, y)
            //              ||        |
            //              ||        |
            //             6\|      5 |
            //               \3------4+

            void CreateCube(Vector3 position, Matrix4x4 basis, float width, ref int lastIdx, ref int lastTriIdx, Color color)
            {
                Vector3 normal = basis.GetColumn(0);
                Vector3 forward = basis.GetColumn(1);
                Vector3 tangent = basis.GetColumn(2);
                // Create vertices
                var v1 = lastIdx;
                var v2 = lastIdx - 1; // should never wrap... so no security because why not
                var v3 = lastIdx - 2;
                var v4 = lastIdx - 3;
                var v5 = NextIdx();
                var v6 = NextIdx();
                var v7 = NextIdx();
                var v8 = NextIdx();

                var halfWidth = width * 0.5f;
                vertices[v8] = position + halfWidth * tangent;
                vertices[v7] = position - halfWidth * tangent;
                vertices[v6] = vertices[v7] - normal * 1;
                vertices[v5] = vertices[v8] - normal * 1;

                // all normal the same as it is not really used
                normals[v8] = normal;
                normals[v7] = normal;
                normals[v6] = normal;
                normals[v5] = normal;

                // same for tangent
                tangents[v8] = tangent;
                tangents[v7] = tangent;
                tangents[v6] = tangent;
                tangents[v5] = tangent;

                // uvs only important for top face, not even sure...
                // texture should repeat
                uvs[v7].x = 0;
                uvs[v7].y = uvs[v1].y + 1;
                uvs[v8].x = 1;
                uvs[v8].y = uvs[v7].y;

                colors[v7] = color;
                colors[v8] = color;

                // Create faces
                // first set back pointer to override last face
                var tIdxNow = tIdx;
                tIdx = (lastTriIdx - 6 + triCount) % triCount;

                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v7;
                triangles[NextTriIdx()] = v8;

                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v2;
                triangles[NextTriIdx()] = v7;

                // set back pointer to previous state and continue
                tIdx = tIdxNow;

                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v7;
                triangles[NextTriIdx()] = v2;

                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v7;

                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v8;

                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v1;

                // todo remove this face should not be necessary
                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v5;

                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v6;

                // finish by back face so it is easily re-used when constructing following marks
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v8;
                triangles[NextTriIdx()] = v7;

                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v8;

                lastIdx = vIdx;
                lastTriIdx = tIdx;

                needUpdateMesh = true;
            }

            void CreateDetachedCube(Vector3 position, Matrix4x4 basis, float width, ref int lastIdx, ref int lastTriIdx, Color color)
            {
                // create a new quad basis
                Vector3 normal = basis.GetColumn(0);
                Vector3 forward = basis.GetColumn(1);
                Vector3 tangent = basis.GetColumn(2);

                // Create vertices
                var v1 = NextIdx();
                var v2 = NextIdx();
                var v3 = NextIdx();
                var v4 = NextIdx();
                var v5 = NextIdx();
                var v6 = NextIdx();
                var v7 = NextIdx();
                var v8 = NextIdx();

                var halfWidth = width * 0.5f;
                vertices[v1] = position + halfWidth * tangent - min_dist_sqr * forward;
                vertices[v2] = position - halfWidth * tangent - min_dist_sqr * forward;
                vertices[v3] = vertices[v2] - normal * 1;
                vertices[v4] = vertices[v1] - normal * 1;
                vertices[v8] = position + halfWidth * tangent;
                vertices[v7] = position - halfWidth * tangent;
                vertices[v6] = vertices[v7] - normal * 1;
                vertices[v5] = vertices[v8] - normal * 1;

                // all normal the same as it is not really used
                normals[v1] = normal;
                normals[v2] = normal;
                normals[v3] = normal;
                normals[v4] = normal;
                normals[v8] = normal;
                normals[v7] = normal;
                normals[v6] = normal;
                normals[v5] = normal;

                // same for tangent
                tangents[v1] = tangent;
                tangents[v2] = tangent;
                tangents[v3] = tangent;
                tangents[v4] = tangent;
                tangents[v8] = tangent;
                tangents[v7] = tangent;
                tangents[v6] = tangent;
                tangents[v5] = tangent;

                // uvs only important for top face, not even sure...
                // texture should repeat
                uvs[v2].x = 0;
                uvs[v2].y = 0;
                uvs[v1].x = 1;
                uvs[v1].y = 0;
                uvs[v7].x = 0;
                uvs[v7].y = uvs[v1].y + 1;
                uvs[v8].x = 1;
                uvs[v8].y = uvs[v7].y;

                colors[v1] = color;
                colors[v2] = color;
                colors[v3] = color;
                colors[v4] = color;
                colors[v5] = color;
                colors[v6] = color;
                colors[v7] = color;
                colors[v8] = color;

                // Create faces
                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v7;
                triangles[NextTriIdx()] = v8;

                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v2;
                triangles[NextTriIdx()] = v7;

                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v7;
                triangles[NextTriIdx()] = v2;

                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v7;

                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v2;
                triangles[NextTriIdx()] = v1;

                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v2;

                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v1;
                triangles[NextTriIdx()] = v8;

                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v1;

                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v5;

                triangles[NextTriIdx()] = v4;
                triangles[NextTriIdx()] = v3;
                triangles[NextTriIdx()] = v6;

                // finish by back face so it is easily re-used when constructing following marks
                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v8;
                triangles[NextTriIdx()] = v7;

                triangles[NextTriIdx()] = v6;
                triangles[NextTriIdx()] = v5;
                triangles[NextTriIdx()] = v8;

                lastIdx = vIdx;
                lastTriIdx = tIdx;

                needUpdateMesh = true;
            }

            void CreateDetachedQuad(Vector3 position, Matrix4x4 basis, float width, ref int lastIdx, Color color)
            {
                // create a new quad basis
                Vector3 normal = basis.GetColumn(0);
                Vector3 forward = basis.GetColumn(1);
                Vector3 right = basis.GetColumn(2);

                var idx1 = NextIdx();
                var idx2 = NextIdx();

                var halfWidth = width * 0.5f;
                vertices[idx1] = position - halfWidth * right - min_dist_sqr * forward;
                vertices[idx2] = position + halfWidth * right - min_dist_sqr * forward;

                normals[idx1] = normal;
                normals[idx2] = normal;

                tangents[idx1] = right;
                tangents[idx2] = right;

                // texture should repeat
                uvs[idx1].x = 0;
                uvs[idx1].y = 0;
                uvs[idx2].x = 1;
                uvs[idx2].y = 0;

                colors[idx1] = color;
                colors[idx2] = color;

                lastIdx = vIdx;

                CreateQuad(position, basis, width, ref lastIdx, color);
            }

            void CreateQuad(Vector3 position, Matrix4x4 basis, float width, ref int lastIdx, Color color)
            {
                // create a new quad connected to last position
                Vector3 right = basis.GetColumn(2);
                Vector3 forward = basis.GetColumn(1);
                Vector3 normal = basis.GetColumn(0);

                var halfWidth = width * 0.5f;
                // var p1 = position + halfWidth * right + halfWidth * forward;
                // var p2 = position - halfWidth * right + halfWidth * forward;
                // var p3 = position + halfWidth * right - halfWidth * forward;
                // var p4 = position - halfWidth * right - halfWidth * forward;
                // Vector3 v1 = Vector3.zero;
                // v1.x = Mathf.Min(Mathf.Min(Mathf.Min(p1.x,p2.x), p3.x ), p4.x);
                // v1.y = Mathf.Min(Mathf.Min(Mathf.Min(p1.y,p2.y), p3.y ), p4.y);
                // v1.z = Mathf.Min(Mathf.Min(Mathf.Min(p1.z,p2.z), p3.z ), p4.z);

                // Vector3 v2 = Vector3.zero;
                // v2.x = Mathf.Max(Mathf.Max(Mathf.Max(p1.x,p2.x), p3.x ), p4.x);
                // v2.y = Mathf.Max(Mathf.Max(Mathf.Max(p1.y,p2.y), p3.y ), p4.y);
                // v2.z = Mathf.Max(Mathf.Max(Mathf.Max(p1.z,p2.z), p3.z ), p4.z);

                var idx1 = NextIdx();
                var idx2 = NextIdx();

                vertices[idx1] = position - halfWidth * right;
                vertices[idx2] = position + halfWidth * right;

                normals[idx1] = normal;
                normals[idx2] = normal;

                tangents[idx1] = right;
                tangents[idx2] = right;

                // texture should repeat
                uvs[idx1].x = 0;
                uvs[idx1].y = uvs[lastIdx].y;
                uvs[idx2].x = uvs[lastIdx].x;
                uvs[idx2].y = uvs[lastIdx].y + 1;

                colors[idx1] = color;
                colors[idx2] = color;

                triangles[tIdx + 0] = lastIdx - 1;
                triangles[tIdx + 1] = idx1;
                triangles[tIdx + 2] = lastIdx;

                triangles[tIdx + 3] = lastIdx;
                triangles[tIdx + 4] = idx1;
                triangles[tIdx + 5] = idx2;

                tIdx = (tIdx + 6) % triCount;

                lastIdx = vIdx;

                needUpdateMesh = true;
            }

            public void AddGroundMark(Vector3 position, Matrix4x4 basis, float width, ref int lastIdx, ref int lastTriIdx, Color color)
            {
                var nextPosition = position;

                //if(vIdx < 0){
                //    CreateDetachedQuad(nextPosition, basis, width, ref lastIdx);
                //    return;
                //}
                var lastPosition = (vertices[lastIdx] + vertices[(lastIdx - 1 + dataCount) % dataCount]) * 0.5f;
                var dir = nextPosition - lastPosition;
                var dist = dir.sqrMagnitude;

                // cull if not far enough yet
                if (dist == 1 || dist < min_dist_sqr)
                    return;

                if (dist > min_dist_sqr * 100)
                    CreateDetachedCube(nextPosition, basis, width, ref lastIdx, ref lastTriIdx, color);
                else
                {
                    basis = Schnibble.SchMathf.GetBasis(basis.GetColumn(0), dir.normalized);
                    CreateCube(nextPosition, basis, width, ref lastIdx, ref lastTriIdx, color);
                }
            }

            public void UpdateMesh()
            {
                if (!needUpdateMesh) return;

                cmd.Clear();

                int normalsid = Shader.PropertyToID("_GBufferNormals");
                cmd.GetTemporaryRT(normalsid, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear, 1, false);
                cmd.Blit(BuiltinRenderTextureType.GBuffer2, normalsid);
                cmd.SetRenderTarget(new RenderTargetIdentifier[4]{
                BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1,
                BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget}, BuiltinRenderTextureType.CameraTarget);
                //cmd.DrawMesh(groundMarksMesh, t.localToWorldMatrix, groundMarksMaterial, 0, 0);
                cmd.DrawMeshInstanced(cube, 0, groundMarksMaterial, 0, objectToWorld);
                cmd.ReleaseTemporaryRT(normalsid);


                groundMarksMesh.vertices = vertices;
                groundMarksMesh.normals = normals;
                groundMarksMesh.tangents = tangents;
                groundMarksMesh.triangles = triangles;
                groundMarksMesh.colors = colors;
                groundMarksMesh.uv = uvs;
                //groundMarksMesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10000, 10000, 10000));

                needUpdateMesh = false;
            }

            public void Init(Camera cam, Transform t)
            {
                vIdx = -1;
                tIdx = -1;

                groundMarksMesh = new Mesh();
                groundMarksMesh.MarkDynamic();
                groundMarksMesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10000, 10000, 10000));


                vertices = new Vector3[dataCount];
                normals = new Vector3[dataCount];
                tangents = new Vector4[dataCount];
                colors = new Color[dataCount];
                uvs = new Vector2[dataCount];
                triangles = new int[triCount];

                objectToWorld = new Matrix4x4[maxDecals];

                cmd = new CommandBuffer();
                cmd.name = "Marks";
                cam.AddCommandBuffer(CameraEvent.BeforeLighting, cmd);
            }
        };


        [System.Serializable]
        public struct LElement
        {
            public PhysicMaterial unityMaterial;
            public SchGround schGroundMaterial;
        };

        public Dictionary<int, SchGround> elements = new Dictionary<int, SchGround>();

        public SchGround GetMat(int hash)
        {
            return elements[hash];
        }

        public void Init(List<LElement> initElements, Camera cam, Transform t)
        {
            foreach (var e in initElements)
            {
                e.schGroundMaterial.Init(cam, t);
                elements.Add(e.unityMaterial.name.GetHashCode(), e.schGroundMaterial);
            }
        }
    }

    public List<PhysicsMaterialManagerData.LElement> initElements;
    // Start is called before the first frame update
    void Start()
    {
        var inst = PhysicsMaterialManager.instance;
        inst.Init(initElements, cam, transform);
    }

    void LateUpdate()
    {
        foreach (var e in initElements)
        {
            e.schGroundMaterial.UpdateMesh();
        }
    }
}
