using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class RemoveMemberFromTeamEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Team Membership Canceled: You have been removed from a team.";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var teamLeadName = data["TeamLeadName"];
        var teamName = data["TeamName"];

        return $@"Hi {assigneeFullName},

You have been removed from {teamName} team by {assignerFullName}.
This team is led by {teamLeadName}.

Regards,
Collabrium Team";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var teamLeadName = data["TeamLeadName"];
        var teamName = data["TeamName"];

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
          <h2 style=""color: #2a3f58; margin-top: 10;"">Team Membership Canceled</h2>
        </td>
      </tr>
      
      <tr>
        <td style=""padding: 40px 30px;"">
          <p style=""color: #555; font-size: 16px; line-height: 1.6;"">
            Hi <strong>{assigneeFullName}</strong>,<br/><br/>
            You have been removed from <strong> {teamName} </strong> team by <strong> {assignerFullName}. </strong>
            this team is led by <strong>{teamLeadName}.</strong>
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