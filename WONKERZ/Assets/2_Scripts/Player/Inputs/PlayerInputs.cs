using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using System;

using Schnibble.Managers;

static public class InputSettings
{
    public static bool InverseCameraMappingX = false;
    public static bool InverseCameraMappingY = false;
    public static float MouseMultiplier = 20;
}

public class WonkGameController : GameController {
    public WonkGameController(int i) : base(i){}
    public override int GetIdx(string name) => PlayerInputs.GetIdx(name);
}

public class Player0Inputs : PlayerInputs
{
    public Player0Inputs()
    {
        _controller = new WonkGameController((int)InputCode.Count);

        GameInputAxis.Settings defaultAxisSettings = new GameInputAxis.Settings(0.1f, 10f);

        // powers
        controller.AddInput((int)InputCode.AirplaneMode, new GameInputButton("Airplane transform", "Transform into an airplane", new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.F)));
        controller.AddInput((int)InputCode.SpinAttack, new GameInputButton("Spin attack", "Launches a spin attack used to damage nearby enemies and deflect projectiles.", new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.E)));
        
        // misc camera
        controller.AddInput((int)InputCode.CameraX,
            new GameInputAxis("CameraX", "Move camera left or right",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightH), new Controller.InputCode(Controller.JoystickAxisCode.RightHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.CameraY,
            new GameInputAxis("CameraY", "Move camera up or down",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightV), new Controller.InputCode(Controller.JoystickAxisCode.RightVNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.CameraChange,
            new GameInputButton("CameraSwitch", "Change camera or keep pressed to reset view behind player", new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg), new Controller.InputCode(KeyCode.A)));
        
        controller.AddInput((int)InputCode.CameraFocus, 
            new GameInputButton("CameraFocus", "Camera focus on nearest focusable object", new Controller.InputCode(Controller.JoystickButtonsCode.Y), new Controller.InputCode(KeyCode.A)));
        
        controller.AddInput((int)InputCode.CameraReset,
            new GameInputButton("CameraReset", "Reset camera behind player", new Controller.InputCode(Controller.JoystickButtonsCode.RS), new Controller.InputCode(KeyCode.Tab)));
        
        // player : car
        controller.AddInput((int)InputCode.Accelerator,
            new GameInputAxis("Accelerator", "Car acceleration",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RT), new Controller.InputCode(Controller.JoystickAxisCode.LT)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.W), new Controller.InputCode(KeyCode.W)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.Break,
            new GameInputAxis("Break", "Car breaks and backwards acceleration",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.LT), new Controller.InputCode(Controller.JoystickAxisCode.LT)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.S), new Controller.InputCode(KeyCode.S)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.Turn,
            new GameInputAxis("Turn", "Car direction",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.LeftH), new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.Handbrake,
            new GameInputButton("Handbrake","Handbrake to slip", new Controller.InputCode(Controller.JoystickButtonsCode.LB), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.Turbo,
            new GameInputButton("Turbo","Turbo", new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.F)));


        // ux

        controller.AddInput((int)InputCode.UIUp,
            new GameInputButtonFromAxis("UIUp", "Menus up",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftV), new Controller.InputCode(KeyCode.W)));


        controller.AddInput((int)InputCode.UIDown,
            new GameInputButtonFromAxis("UIDown", "Menus down",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftVNeg), new Controller.InputCode(KeyCode.S)));


        controller.AddInput((int)InputCode.UILeft,
            new GameInputButtonFromAxis("UILeft", "Menus left",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg), new Controller.InputCode(KeyCode.A)));

        controller.AddInput((int)InputCode.UIRight,
            new GameInputButtonFromAxis("UIRight", "Menus right",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftH), new Controller.InputCode(KeyCode.D)));


        controller.AddInput((int)InputCode.UICancel,
            new GameInputButton("UICancel", "Menus cancel",
                new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.Escape)));


        controller.AddInput((int)InputCode.UIValidate,
            new GameInputButton("UIValidate", "Menus validation",
                new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.Return)));


        controller.AddInput((int)InputCode.UIStart,
            new GameInputButton("UIStart", "Menus pause",
                new Controller.InputCode(Controller.JoystickButtonsCode.Start), new Controller.InputCode(KeyCode.Escape)));
    }
}

public class Player1Inputs : PlayerInputs
{
    public Player1Inputs()
    {
        _controller = new WonkGameController((int)InputCode.Count);

        GameInputAxis.Settings defaultAxisSettings = new GameInputAxis.Settings(0.1f, 10f);

        // powers
        controller.AddInput((int)InputCode.AirplaneMode, new GameInputButton("Airplane transform", "Transform into an airplane", new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.F)));
        controller.AddInput((int)InputCode.SpinAttack, new GameInputButton("Spin attack", "Launches a spin attack used to damage nearby enemies and deflect projectiles.", new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.E)));

        // misc camera
        controller.AddInput((int)InputCode.CameraX,
            new GameInputAxis("CameraX", "Move camera left or right",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightH), new Controller.InputCode(Controller.JoystickAxisCode.RightHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.CameraY,
            new GameInputAxis("CameraY", "Move camera up or down",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightV), new Controller.InputCode(Controller.JoystickAxisCode.RightVNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));
            
        controller.AddInput((int)InputCode.CameraChange,
            new GameInputButton("CameraSwitch", "Change camera or keep pressed to reset view behind player", new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg), new Controller.InputCode(KeyCode.A)));

        controller.AddInput((int)InputCode.CameraFocus, 
            new GameInputButton("CameraFocus", "Camera focus on nearest focusable object", new Controller.InputCode(Controller.JoystickButtonsCode.Y), new Controller.InputCode(KeyCode.A)));

        controller.AddInput((int)InputCode.CameraReset,
            new GameInputButton("CameraReset", "Reset camera behind player", new Controller.InputCode(Controller.JoystickButtonsCode.RS), new Controller.InputCode(KeyCode.Tab)));

        // misc : save

        controller.AddInput((int)InputCode.SaveStatesPlant,
            new GameInputButtonFromAxis("SaveStatesPlant", "Plant save point", new Controller.InputCode(Controller.JoystickAxisCode.DpadV), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.SaveStatesReturn,
            new GameInputButtonFromAxis("SaveStatesReturn", "Return to save point", new Controller.InputCode(Controller.JoystickAxisCode.DpadVNeg), new Controller.InputCode(KeyCode.F)));

        controller.AddInput((int)InputCode.GiveCoinsForTurbo,
            new GameInputButtonFromAxis("GiveCoinsForTurbo","Exchange coin at gas station", new Controller.InputCode(Controller.JoystickAxisCode.DpadH), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.Jump,
            new GameInputButton("Jump","Jump (maintainable)", new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.WeightX,
            new GameInputAxis("WeightX", "Car weight and aerials control roll left/right",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightH), new Controller.InputCode(Controller.JoystickAxisCode.RightHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.WeightY,
            new GameInputAxis("WeightY", "Car weight and aerials control roll control/back",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightV), new Controller.InputCode(Controller.JoystickAxisCode.RightVNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        // ux

        controller.AddInput((int)InputCode.UIUp,
            new GameInputButtonFromAxis("UIUp", "Menus up",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftV), new Controller.InputCode(KeyCode.W)));


        controller.AddInput((int)InputCode.UIDown,
            new GameInputButtonFromAxis("UIDown", "Menus down",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftVNeg), new Controller.InputCode(KeyCode.S)));


        controller.AddInput((int)InputCode.UILeft,
            new GameInputButtonFromAxis("UILeft", "Menus left",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg), new Controller.InputCode(KeyCode.A)));

        controller.AddInput((int)InputCode.UIRight,
            new GameInputButtonFromAxis("UIRight", "Menus right",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftH), new Controller.InputCode(KeyCode.D)));


        controller.AddInput((int)InputCode.UICancel,
            new GameInputButton("UICancel", "Menus cancel",
                new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.Escape)));


        controller.AddInput((int)InputCode.UIValidate,
            new GameInputButton("UIValidate", "Menus validation",
                new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.Return)));

        controller.AddInput((int)InputCode.UIStart,
            new GameInputButton("UIStart", "Menus pause",
                new Controller.InputCode(Controller.JoystickButtonsCode.Start), new Controller.InputCode(KeyCode.Escape)));
    }

}

public class PlayerInputs
{
    public enum InputCode
    {
        // must be consequent order!
        // buttons

        // player : misc
        CameraX = 0,
        CameraY,
        // player: car
        Accelerator,
        Break,
        Turn,
        WeightY,
        WeightX,
        ForwardBackward,
        // player : misc

        AirplaneMode,
        SpinAttack,

        // axis
        CameraChange,
        CameraReset,
        CameraFocus,
        SaveStatesPlant,
        SaveStatesReturn,
        GiveCoinsForTurbo,
        CameraFocusChange,
        // player: car
        Handbrake,
        Jump,
        Turbo,
        // ux
        UIUp,
        UIDown,
        UILeft,
        UIRight,
        UIValidate,
        UICancel,
        UIStart,
        // test
        Grapin,
        Count,
    }

    // PERF : probably to avoid in hot path
    public static int GetIdx(string enumCode){
        return (int)Enum.Parse(typeof(InputCode), enumCode);
    }

    protected GameController _controller;
    public GameController controller { get => _controller; }

    public PlayerInputs()
    {
        _controller = new WonkGameController((int)InputCode.Count);

        GameInputAxis.Settings defaultAxisSettings = new GameInputAxis.Settings(0.01f, 10f);

        // powers
        controller.AddInput((int)InputCode.AirplaneMode, new GameInputButton("Airplane transform", "Transform into an airplane", new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.F)));
        controller.AddInput((int)InputCode.SpinAttack, new GameInputButton("Spin attack", "Launches a spin attack used to damage nearby enemies and deflect projectiles.", new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.E)));

        // misc camera
        controller.AddInput((int)InputCode.CameraX,
            new GameInputAxis("CameraX", "Move camera left or right",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.DpadH), new Controller.InputCode(Controller.JoystickAxisCode.DpadHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.CameraY,
            new GameInputAxis("CameraY", "Move camera up or down",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.DpadV), new Controller.InputCode(Controller.JoystickAxisCode.DpadVNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(Controller.MouseCode.Cursor), new Controller.InputCode(Controller.MouseCode.Cursor)),
                defaultAxisSettings));
        
        controller.AddInput((int)InputCode.CameraFocus, 
            new GameInputButton("CameraFocus", "Camera focus on nearest focusable object", new Controller.InputCode(Controller.JoystickButtonsCode.Y), new Controller.InputCode(KeyCode.A)));
        
        controller.AddInput((int)InputCode.CameraReset,
            new GameInputButton("CameraReset", "Reset camera behind player", new Controller.InputCode(Controller.JoystickButtonsCode.RS), new Controller.InputCode(KeyCode.Tab)));

        controller.AddInput((int)InputCode.CameraFocusChange,
            new GameInputAxis("CameraFocusChange","Change the current camera focus",                 
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.DpadH), new Controller.InputCode(Controller.JoystickAxisCode.DpadHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.O), new Controller.InputCode(KeyCode.L)),
                defaultAxisSettings));


        // misc : save
        controller.AddInput((int)InputCode.SaveStatesPlant,
            new GameInputButton("SaveStatesPlant", "Plant save point", new Controller.InputCode(Controller.JoystickButtonsCode.Select), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.SaveStatesReturn,
            new GameInputButton("SaveStatesReturn", "Return to save point", new Controller.InputCode(Controller.JoystickButtonsCode.X), new Controller.InputCode(KeyCode.F)));

        controller.AddInput((int)InputCode.GiveCoinsForTurbo,
            new GameInputButton("GiveCoinsForTurbo","Exchange coin at gas station", new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.F)));

        // player : car
        controller.AddInput((int)InputCode.Accelerator,
            new GameInputAxis("Accelerator", "Car acceleration",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RT), new Controller.InputCode(Controller.JoystickAxisCode.LT)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.W), new Controller.InputCode(KeyCode.W)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.Break,
            new GameInputAxis("Break", "Car breaks and backwards acceleration",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.LT), new Controller.InputCode(Controller.JoystickAxisCode.LT)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.S), new Controller.InputCode(KeyCode.S)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.Turn,
            new GameInputAxis("Turn", "Car direction",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.LeftH), new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));


        controller.AddInput((int)InputCode.ForwardBackward,
            new GameInputAxis("Turn", "Car Up/Down shifting",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.LeftV), new Controller.InputCode(Controller.JoystickAxisCode.LeftVNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.Handbrake,
            new GameInputButton("Handbrake","Handbrake to slip", new Controller.InputCode(Controller.JoystickButtonsCode.LB), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.Turbo,
            new GameInputButton("Turbo","Turbo", new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.Jump,
            new GameInputButton("Jump","Jump (maintainable)", new Controller.InputCode(Controller.JoystickButtonsCode.RB), new Controller.InputCode(KeyCode.F)));


        controller.AddInput((int)InputCode.WeightX,
            new GameInputAxis("WeightX", "Car weight and aerials control roll left/right",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightH), new Controller.InputCode(Controller.JoystickAxisCode.RightHNeg)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        controller.AddInput((int)InputCode.WeightY,
            new GameInputAxis("WeightY", "Car weight and aerials control roll control/back",
                new GameInputAxis.Axis(new Controller.InputCode(Controller.JoystickAxisCode.RightVNeg), new Controller.InputCode(Controller.JoystickAxisCode.RightV)),
                new GameInputAxis.Axis(new Controller.InputCode(KeyCode.A), new Controller.InputCode(KeyCode.D)),
                defaultAxisSettings));

        // ux

        controller.AddInput((int)InputCode.UIUp,
            new GameInputButtonFromAxis("UIUp", "Menus up",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftV), new Controller.InputCode(KeyCode.W)));


        controller.AddInput((int)InputCode.UIDown,
            new GameInputButtonFromAxis("UIDown", "Menus down",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftVNeg), new Controller.InputCode(KeyCode.S)));


        controller.AddInput((int)InputCode.UILeft,
            new GameInputButtonFromAxis("UILeft", "Menus left",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftHNeg), new Controller.InputCode(KeyCode.A)));

        controller.AddInput((int)InputCode.UIRight,
            new GameInputButtonFromAxis("UIRight", "Menus right",
                new Controller.InputCode(Controller.JoystickAxisCode.LeftH), new Controller.InputCode(KeyCode.D)));


        controller.AddInput((int)InputCode.UICancel,
            new GameInputButton("UICancel", "Menus cancel",
                new Controller.InputCode(Controller.JoystickButtonsCode.B), new Controller.InputCode(KeyCode.Escape)));


        controller.AddInput((int)InputCode.UIValidate,
            new GameInputButton("UIValidate", "Menus validation",
                new Controller.InputCode(Controller.JoystickButtonsCode.A), new Controller.InputCode(KeyCode.Return)));


        controller.AddInput((int)InputCode.UIStart,
            new GameInputButton("UIStart", "Menus pause",
                new Controller.InputCode(Controller.JoystickButtonsCode.Start), new Controller.InputCode(KeyCode.Escape)));
    }
}
