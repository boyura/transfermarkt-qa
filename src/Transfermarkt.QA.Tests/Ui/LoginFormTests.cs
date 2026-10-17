using Transfermarkt.QA.Tests.Core;
using Transfermarkt.QA.Tests.Pages;
using Transfermarkt.QA.Tests.Pages.Components;

namespace Transfermarkt.QA.Tests.Ui;

[TestFixture, Category("E2E")]
public class LoginFormTests : BaseUiTest
{
    private const string ValidUsername = "qa_user";
    private const string ValidPassword = "not-a-real-password";

    private HomePage _home = null!;

    private LoginFormComponent Login => _home.Header.LoginForm;

    [Test(Description = "Login form: initial state, required fields, remember-me, enabled submit, username validation, show password and close")]
    public async Task Login_Form_Behaviour()
    {
        _home = new HomePage(Page);
        await _home.OpenAsync();
        await Expect(Login.Form).ToBeHiddenAsync();

        await _home.Header.LoginButton.ClickAsync();
        await Expect(Login.Form).ToBeVisibleAsync();
        // Initial state
        await Expect(Login.Username).ToBeEmptyAsync();
        await Expect(Login.Password).ToBeEmptyAsync();
        await Expect(Login.Password).ToHaveAttributeAsync("type", "password");
        await Expect(Login.RememberMe).Not.ToBeCheckedAsync();
        await Expect(Login.SubmitButton).ToBeDisabledAsync();
        await Expect(Login.ForgotDetailsLink).ToHaveAttributeAsync("href", new Regex("/profil/loginDetails"));

        // Username and Password are required: Login stays disabled while either is empty
        await Expect(Login.UsernameLabel).ToHaveTextAsync("Username*");
        await Expect(Login.PasswordLabel).ToHaveTextAsync("Password*");
        await Login.Username.FillAsync(ValidUsername);
        await Expect(Login.SubmitButton).ToBeDisabledAsync();
        await Login.Username.ClearAsync();
        await Login.Password.FillAsync(ValidPassword);
        await Expect(Login.SubmitButton).ToBeDisabledAsync();

        // Both filled: Login is enabled
        await Login.Username.FillAsync(ValidUsername);
        await Expect(Login.SubmitButton).ToBeEnabledAsync();

        // Remember me can be checked
        await Login.RememberMe.CheckAsync();
        await Expect(Login.RememberMe).ToBeCheckedAsync();

        // Unknown user: submitting shows an error and the form stays open
        await Expect(Login.LoginError).ToBeHiddenAsync();
        await Login.SubmitButton.ClickAsync();
        await Expect(Login.LoginError).ToBeVisibleAsync();
        await Expect(Login.LoginError).ToHaveTextAsync(
            "Incorrect username or password. Note: You can only log in with your username, not your email address.");
        await Expect(Login.Form).ToBeVisibleAsync();

        // Close button dismisses the form
        await Login.CloseButton.ClickAsync();
        await Expect(Login.Form).ToBeHiddenAsync();
    }
}
