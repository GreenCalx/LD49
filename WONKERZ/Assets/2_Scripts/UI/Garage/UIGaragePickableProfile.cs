using Schnibble;

public class UIGaragePickableProfile : UITextTab
{
    public string profile_name;

    override protected void ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        // dont execute base ProcessInputs
        // >>
        // Waiting for new wheels with partcolorizable on them to reactivate this feature
        // ATM it crashes on the wheels serialization (obviously)

        // if ((Entry[(int) PlayerInputs.InputCode.UIValidate] as GameInputButton).GetState().down)
        // (Parent as UIGarageProfilePanel).save(profile_name);

        // if ((Entry[(int) PlayerInputs.InputCode.CameraFocus] as GameInputButton).GetState().down)
        // (Parent as UIGarageProfilePanel).load(profile_name);
    }

}
