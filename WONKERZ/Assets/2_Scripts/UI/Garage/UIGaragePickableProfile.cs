using Schnibble;

public class UIGaragePickableProfile : UITextTab
{
    public string profile_name;

    override protected void ProcessInputs(InputManager currentMgr, GameInput[] Inputs)
    {
        // dont execute base ProcessInputs
        // if ((Entry[(int) PlayerInputs.InputCode.Jump] as GameInputButton).GetState().down)
        // (Parent as UIGarageProfilePanel).save(profile_name);

        // if ((Entry[(int) PlayerInputs.InputCode.CameraChange] as GameInputButton).GetState().down)
        // (Parent as UIGarageProfilePanel).load(profile_name);
    }

}
