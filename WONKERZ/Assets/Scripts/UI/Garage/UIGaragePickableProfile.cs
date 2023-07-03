
public class UIGaragePickableProfile : UITextTab
{
    public string profile_name;

    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        // dont execute base ProcessInputs
        if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
            (Parent as UIGarageProfilePanel).save(profile_name);

        if (Entry.Inputs[Constants.INPUT_CAMERACHANGE].IsDown)
            (Parent as UIGarageProfilePanel).load(profile_name);
    }

}
