using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class ConfirmationEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Confirm your email";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var fullName = data["FullName"];
        var url = data["newUrl"];

        return $@"Hi {fullName},

Thanks for signing up! Please confirm your account by visiting this link: {url}

If you did not request this, you can safely ignore this email.";
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
          <h2 style=""color: #333;"">Confirm Your Email</h2>
          <p style=""color: #555; font-size: 16px;"">
            Hi {fullName},<br/><br/>
            Thanks for signing up! Please confirm your account by clicking the button below.
          </p>
          <p style=""margin: 30px 0;"">
            <a href=""{url}"" 
               style=""background: #ECBF28; color: #fff; text-decoration: none; 
                      padding: 12px 24px; border-radius: 6px; font-weight: bold;"">
              Confirm Email
            </a>
          </p>
          <hr style=""margin: 30px 0; border: none; border-top: 1px solid #eee;"" />
          <p style=""color: #aaa; font-size: 12px;"">
            If you didn’t create an account, you can safely ignore this email.
          </p>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}
