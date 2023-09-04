
public class UIGaragePickableProfile : UITextTab
{
    public string profile_name;

    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        // dont execute base ProcessInputs
        if (Entry.Inputs[(int) GameInputsButtons.Jump].IsDown)
        (Parent as UIGarageProfilePanel).save(profile_name);

        if (Entry.Inputs[(int) GameInputsButtons.CameraChange].IsDown)
        (Parent as UIGarageProfilePanel).load(profile_name);
    }

}
