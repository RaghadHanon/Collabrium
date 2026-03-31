using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class UserRoleChangedEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "New Job Title Assigned to you";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var newJobTitle = data["NewJobTitle"];
        var oldJobTitle = data["OldJobTitle"];

        return $@"Hi {assigneeFullName},

Your Job Title has been changed by {assignerFullName} from {oldJobTitle} to {newJobTitle}.

Regards,
Collabrium Team";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var newJobTitle = data["NewJobTitle"];
        var oldJobTitle = data["OldJobTitle"];

        return $@"
<html>
  <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f6; padding: 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 12px; 
                  box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden;"">
      <tr>
        <td style=""text-align: center; padding: 20px 0px"">
           <img src=""https://res.cloudinary.com/dcfpagr5v/image/upload/v1772752623/Collabrium/ProfileImages/mqogyk5po3aoq7gv2oov.jpg"" 
                alt=""Collabrium Logo"" style=""display: block; margin: 0 auto; width: 150px; height: 150px; 
                         border-radius: 50%; object-fit: cover; -webkit-border-radius: 50%; -moz-border-radius: 50%;"" />
          <h2 style=""color: #2a3f58; margin-top: 10;"">New Job Title Assigned to you</h2>
        </td>
      </tr>
      
      <tr>
        <td style=""padding: 40px 30px;"">
          <p style=""color: #555; font-size: 16px; line-height: 1.6;"">
            Hi <strong>{assigneeFullName}</strong>,<br/><br/>
            Your Job Title has been changed by <strong>{assignerFullName}</strong> from <strong>{oldJobTitle}</strong> to <strong>{newJobTitle}</strong>.
          </p>

          <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"" />
          <p style=""color: #888; font-size: 13px; text-align: center;"">
            This is an automated notification from Collabrium. 
            Please do not reply directly to this email.
          </p>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}