using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class PasswordResetEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Reset Your Password";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var fullName = data["FullName"];
        var url = data["newUrl"];

        return $@"Hi {fullName},

We received a request to reset your password. Please click the link below to set a new password: {url}

If you did not request a password reset, you can safely ignore this email.";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var fullName = data["FullName"];
        var url = data["newUrl"];

        return $@"
<html>
  <body style=""font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; 
                  box-shadow: 0 2px 6px rgba(0,0,0,0.1);"">
      <tr>
        <td style=""padding: 30px; text-align: center;"">    
          <img src=""https://res.cloudinary.com/dcfpagr5v/image/upload/v1772752623/Collabrium/ProfileImages/mqogyk5po3aoq7gv2oov.jpg"" alt=""Confirm Email"" width=""150px"" height=""150px""/>
          <h2 style=""color: #333;"">Reset Your Password</h2>
          <p style=""color: #555; font-size: 16px;"">
            Hi {fullName},<br/><br/>
            We received a request to reset your password. Click the button below to set a new password.
          </p>
          <p style=""margin: 30px 0;"">
            <a href=""{url}"" 
               style=""background: #ECBF28; color: #fff; text-decoration: none; 
                      padding: 12px 24px; border-radius: 6px; font-weight: bold;"">
              Reset Password
            </a>
          </p>
          <hr style=""margin: 30px 0; border: none; border-top: 1px solid #eee;"" />
          <p style=""color: #aaa; font-size: 12px;"">
            If you did not request a password reset, you can safely ignore this email.
          </p>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}
