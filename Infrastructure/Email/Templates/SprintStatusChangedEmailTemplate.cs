using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class SprintStatusChangedEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Sprint Status Changed";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var projectKey = data["ProjectKey"];
        var projectTitle = data["ProjectName"];
        var sprintCode = data["SprintCode"];
        var sprintName = data["SprintName"];
        var sprintStatus = data["SprintStatus"];
        var url = data["Url"];

        return $@"Hi {assigneeFullName},

{sprintCode} {sprintName} status has been changed in {projectKey} - {projectTitle} by {assignerFullName}.

Details:
- Project: {projectKey} - {projectTitle}
- Sprint: {sprintCode} {sprintName}
- New Status: {sprintStatus}

You can view the details here: {url}

Regards,
Collabrium Team";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var projectKey = data["ProjectKey"];
        var projectTitle = data["ProjectName"];
        var sprintCode = data["SprintCode"];
        var sprintName = data["SprintName"];
        var sprintStatus = data["SprintStatus"];
        var url = data["Url"];

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
          <h2 style=""color: #2a3f58; margin-top: 10;"">Sprint Status Changed</h2>
        </td>
      </tr>
      
      <tr>
        <td style=""padding: 40px 30px;"">
          <p style=""color: #555; font-size: 16px; line-height: 1.6;"">
            Hi <strong>{assigneeFullName}</strong>,<br/><br/>
             <strong> {sprintCode} {sprintName}</strong> status has been changed in <strong>{projectKey} - {projectTitle} by {assignerFullName}.</strong>. 
            Below are the details:
          </p>
          
          <div style=""background: #f9f9f9; border-left: 4px solid #ECBF28; padding: 15px; margin: 25px 0;"">
            <p style=""margin: 0; color: #555;""><strong>Project:</strong> {projectKey} - {projectTitle}</p>
            <p style=""margin: 0; color: #555;""><strong>Sprint:</strong> {sprintCode} {sprintName}</p>
            <p style=""margin: 0; color: #555;""><strong>New Status:</strong> {sprintStatus}</p>
          </div>

          <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{url}"" 
               style=""background: #ECBF28; color: #fff; text-decoration: none; 
                      padding: 14px 30px; border-radius: 6px; font-weight: bold; display: inline-block;"">
              View Sprint
            </a>
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