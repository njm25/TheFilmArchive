using System.Net;

namespace Infrastructure.Email;

// Email clients are roughly a decade behind browsers: layout has to be tables,
// styling has to be inline, and there is no flexbox, grid, or custom property
// support. Gmail also strips <style> blocks entirely, so nothing here may depend
// on one - every rule that matters is inline on the element.
//
// The palette and type mirror apps/client/src/styles.css so mail from the site
// reads as the same product. Keep the two in step if the site's theme changes.
public static class EmailTemplates
{
    private const string Bg = "#111111";
    private const string Surface = "#1c1c1c";
    private const string Border = "#3a3a3a";
    private const string Text = "#f0f0f0";
    private const string TextMuted = "#a0a0a0";
    private const string TextDim = "#606060";
    private const string Accent = "#c9a84c";

    // Text sitting on the gold accent - the site pairs it with the near-black ground.
    private const string OnAccent = "#111111";

    private const string FontSerif = "Georgia, 'Times New Roman', Times, serif";
    private const string FontSans = "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";

    public static string RegistrationInvite(string siteUrl, string registrationLink)
    {
        string link = WebUtility.HtmlEncode(registrationLink);
        string home = WebUtility.HtmlEncode(siteUrl);

        string content =
            $"""
             <h1 style="margin:0 0 16px;font-family:{FontSerif};font-size:24px;line-height:1.25;font-weight:normal;color:{Text};">
               Finish creating your account
             </h1>
             <p style="margin:0 0 24px;font-family:{FontSans};font-size:15px;line-height:1.6;color:{TextMuted};">
               You asked for an account on The Film Archive. Choose a username and password to finish setting it up.
             </p>
             <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin:0 0 28px;">
               <tr>
                 <td bgcolor="{Accent}" style="border-radius:4px;">
                   <a href="{link}" style="display:inline-block;padding:13px 30px;font-family:{FontSans};font-size:15px;font-weight:600;color:{OnAccent};text-decoration:none;border-radius:4px;">
                     Create your account
                   </a>
                 </td>
               </tr>
             </table>
             <p style="margin:0 0 8px;font-family:{FontSans};font-size:13px;line-height:1.5;color:{TextDim};">
               Or paste this link into your browser:
             </p>
             <p style="margin:0 0 24px;font-family:{FontSans};font-size:13px;line-height:1.5;word-break:break-all;">
               <a href="{link}" style="color:{Accent};text-decoration:underline;">{link}</a>
             </p>
             <p style="margin:0;font-family:{FontSans};font-size:13px;line-height:1.5;color:{TextDim};">
               This link expires in 24 hours.
             </p>
             """;

        return Layout(
            title: "Finish creating your Film Archive account",
            preheader: "Your registration link expires in 24 hours.",
            siteUrl: home,
            contentHtml: content,
            footerHtml: "If you didn't request an account, you can safely ignore this email."
        );
    }

    public static string PasswordReset(string siteUrl, string resetLink)
    {
        string link = WebUtility.HtmlEncode(resetLink);
        string home = WebUtility.HtmlEncode(siteUrl);

        string content =
            $"""
             <h1 style="margin:0 0 16px;font-family:{FontSerif};font-size:24px;line-height:1.25;font-weight:normal;color:{Text};">
               Reset your password
             </h1>
             <p style="margin:0 0 24px;font-family:{FontSans};font-size:15px;line-height:1.6;color:{TextMuted};">
               Someone asked to reset the password for your Film Archive account. Choose a new one using the link below.
             </p>
             <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin:0 0 28px;">
               <tr>
                 <td bgcolor="{Accent}" style="border-radius:4px;">
                   <a href="{link}" style="display:inline-block;padding:13px 30px;font-family:{FontSans};font-size:15px;font-weight:600;color:{OnAccent};text-decoration:none;border-radius:4px;">
                     Choose a new password
                   </a>
                 </td>
               </tr>
             </table>
             <p style="margin:0 0 8px;font-family:{FontSans};font-size:13px;line-height:1.5;color:{TextDim};">
               Or paste this link into your browser:
             </p>
             <p style="margin:0 0 24px;font-family:{FontSans};font-size:13px;line-height:1.5;word-break:break-all;">
               <a href="{link}" style="color:{Accent};text-decoration:underline;">{link}</a>
             </p>
             <p style="margin:0;font-family:{FontSans};font-size:13px;line-height:1.5;color:{TextDim};">
               This link expires in an hour and can only be used once. If you didn't ask for it, ignore this email - your password stays as it is.
             </p>
             """;

        return Layout(
            title: "Reset your Film Archive password",
            preheader: "Your reset link expires in an hour.",
            siteUrl: home,
            contentHtml: content,
            footerHtml: "If you didn't request a password reset, you can safely ignore this email."
        );
    }

    // Sent when someone asks for a link on an address that already has an
    // account. The sign-up response is identical either way, so this is what
    // keeps the flow honest for the real owner without telling the requester
    // whether the address is registered.
    public static string AccountAlreadyExists(string siteUrl)
    {
        string home = WebUtility.HtmlEncode(siteUrl);
        string loginUrl = WebUtility.HtmlEncode($"{siteUrl}/login");

        string content =
            $"""
             <h1 style="margin:0 0 16px;font-family:{FontSerif};font-size:24px;line-height:1.25;font-weight:normal;color:{Text};">
               You already have an account
             </h1>
             <p style="margin:0 0 24px;font-family:{FontSans};font-size:15px;line-height:1.6;color:{TextMuted};">
               Someone asked for a sign-up link for this address, but it's already registered. You can sign in with your existing password.
             </p>
             <table role="presentation" border="0" cellpadding="0" cellspacing="0" style="margin:0 0 28px;">
               <tr>
                 <td bgcolor="{Accent}" style="border-radius:4px;">
                   <a href="{loginUrl}" style="display:inline-block;padding:13px 30px;font-family:{FontSans};font-size:15px;font-weight:600;color:{OnAccent};text-decoration:none;border-radius:4px;">
                     Sign in
                   </a>
                 </td>
               </tr>
             </table>
             <p style="margin:0;font-family:{FontSans};font-size:13px;line-height:1.5;color:{TextDim};">
               If this wasn't you, no action is needed - your account hasn't changed.
             </p>
             """;

        return Layout(
            title: "You already have a Film Archive account",
            preheader: "This address is already registered.",
            siteUrl: home,
            contentHtml: content,
            footerHtml: "You received this because someone entered this address on The Film Archive."
        );
    }

    private static string Layout(
        string title,
        string preheader,
        string siteUrl,
        string contentHtml,
        string footerHtml
    )
    {
        return $"""
        <!doctype html>
        <html lang="en">
        <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="color-scheme" content="dark light">
        <meta name="supported-color-schemes" content="dark light">
        <title>{WebUtility.HtmlEncode(title)}</title>
        </head>
        <body style="margin:0;padding:0;background-color:{Bg};">
          <!-- Inbox preview line; hidden in the body itself. -->
          <div style="display:none;max-height:0;overflow:hidden;mso-hide:all;">{WebUtility.HtmlEncode(preheader)}</div>

          <table role="presentation" border="0" cellpadding="0" cellspacing="0" width="100%" bgcolor="{Bg}" style="background-color:{Bg};">
            <tr>
              <td align="center" style="padding:32px 16px;">

                <table role="presentation" border="0" cellpadding="0" cellspacing="0" width="600" style="width:100%;max-width:600px;border-collapse:collapse;">

                  <!-- Wordmark: the site sets its logo in the serif face. -->
                  <tr>
                    <td align="center" style="padding:0 0 20px;">
                      <a href="{siteUrl}" style="font-family:{FontSerif};font-size:19px;letter-spacing:0.01em;color:{Accent};text-decoration:none;">
                        The Film Archive
                      </a>
                    </td>
                  </tr>

                  <tr>
                    <td bgcolor="{Surface}" style="background-color:{Surface};border:1px solid {Border};border-radius:6px;padding:36px 34px;">
                      {contentHtml}
                    </td>
                  </tr>

                  <tr>
                    <td align="center" style="padding:20px 12px 0;font-family:{FontSans};font-size:12px;line-height:1.6;color:{TextDim};">
                      {footerHtml}
                    </td>
                  </tr>

                </table>

              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }
}
