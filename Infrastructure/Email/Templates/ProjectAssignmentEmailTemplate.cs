using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class ProjectAssignmentEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Project Assignment: A new Project has been assigned to your team";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var teamName = data["TeamName"];
        var projectKey = data["ProjectKey"];
        var projectTitle = data["ProjectName"];
        var url = data["Url"];

        return $@"Hi {assigneeFullName},

A new project has been assigned to your team {teamName} by {assignerFullName}.

Details:
- Key: {projectKey}
- Title: {projectTitle}

You can view the details here: {url}

Regards,
Collabrium Team";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var teamName = data["TeamName"];
        var projectKey = data["ProjectKey"];
        var projectTitle = data["ProjectName"];
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
          <h2 style=""color: #2a3f58; margin-top: 10;"">New Project Assigned</h2>
        </td>
      </tr>
      
      <tr>
        <td style=""padding: 40px 30px;"">
          <p style=""color: #555; font-size: 16px; line-height: 1.6;"">
            Hi <strong>{assigneeFullName}</strong>,<br/><br/>
            You have been assigned a new project #{projectKey} - {projectTitle} by <strong>{assignerFullName} as a member of {teamName} team.</strong>. 
            Below are the details of your new project:
          </p>

          <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{url}"" 
               style=""background: #ECBF28; color: #fff; text-decoration: none; 
                      padding: 14px 30px; border-radius: 6px; font-weight: bold; display: inline-block;"">
              View Project
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