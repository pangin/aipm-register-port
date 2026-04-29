namespace AipmRegister.Core.Notification;

/// CLI/GUI/test surface for user-facing messages. Replaces the MessageBox.Show
/// and inline label updates in frmMain so the workflow code stays UI-agnostic.
public interface IUserNotifier
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? cause = null);

    /// Emits a progress message tagged with the current pipeline stage.
    /// Concrete implementations may color-code by stage in CLI/GUI.
    void Progress(RegistrationStage stage, string message);
}
