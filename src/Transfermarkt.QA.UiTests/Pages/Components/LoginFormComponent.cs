namespace Transfermarkt.QA.UiTests.Pages.Components;

public sealed class LoginFormComponent(IPage page)
{
    public ILocator Form => page.Locator("form.login-form");

    public ILocator Username => Form.Locator("#username");

    public ILocator Password => Form.Locator("#password");

    public ILocator UsernameLabel => Form.Locator("label[for='username']");

    public ILocator PasswordLabel => Form.Locator("label[for='password']");

    public ILocator RememberMe => Form.Locator("#LoginForm_rememberMe");

    public ILocator SubmitButton => Form.Locator("button.tm-btn__login");

    /// <summary>Eye icon inside the password field. One button: only its aria-label/pressed state flips between show and hide.</summary>
    public ILocator ShowPasswordToggle => Form.Locator(".input-password button.tm-input__toggle");

    public ILocator HidePasswordToggle => ShowPasswordToggle;

    public ILocator ForgotDetailsLink => Form.Locator(".button-group a");

    public ILocator LoginError => Form.Locator(".form-group .error-list");

    public ILocator CloseButton => page.Locator("tm-userbox button.cancel-button");

    public async Task FillAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
    }
}
