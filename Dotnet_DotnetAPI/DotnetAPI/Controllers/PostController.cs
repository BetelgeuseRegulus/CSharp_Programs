using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Controller")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId, int userId, string searchParam = "None")
        {
            string sql = @"TutorialAppSchema.spPosts_Get";
            string parameter = "";

            if (postId != 0)
            {
                parameter += ", @PostId=" + postId;
            }

            if (userId != 0)
            {
                parameter += ", @UserId=" + userId;
            }

            if (searchParam != "None")
            {
                parameter += ", @SearchValue='" + searchParam + "'";
            }

            if (parameter.Length > 0)
            {
                sql += parameter.Substring(1);
            }

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql =
                @"EXEC TutorialAppSchema.spPosts_Get @UserId= "
                + this.User.FindFirst("userId")?.Value; // this = controller

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPost("Post")]
        public IActionResult AddPost(Post postToUpsert)
        {
            string sql =
                @"EXEC TutorialAppSchema.spPosts_Upsert"
                + "@UserId="
                + this.User.FindFirst("UserId")?.Value
                + ", @PostTitle='"
                + postToUpsert.PostTitle
                + "', @PostContent='"
                + postToUpsert.PostContent
                + "'";

            if (postToUpsert.PostId > 0)
            {
                sql += ", @PostId=" + postToUpsert.PostId;
            }

            if (_dapper.ExecuteSql(sql))
                return Ok();

            throw new Exception("Failed to upsert post!");
        }

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> PostsBySearch(string searchParam)
        {
            string sql =
                @"SELECT 
                    [PostId],
                    [UserId],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated],
                    FROM TutorialAppSchema.Posts
                    WHERE PostTitle LIKE '%"
                + searchParam
                + "%'"
                + " OR PostContent LIKE '%"
                + searchParam
                + "%'";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPut("Post")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {
            string sql =
                @"
            UPDATE TutorialAppSchema.Posts
                SET PostContent = '"
                + postToEdit.PostContent
                + "', PostTitle = '"
                + postToEdit.PostTitle
                + @"', Postupdated = GETDATE() 
                WHERE PostId = "
                + postToEdit.PostId.ToString()
                + "AND UserId = "
                + this.User.FindFirst("userId")?.Value;

            if (_dapper.ExecuteSql(sql))
                return Ok();

            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql =
                @"EXEC TutorialAppSchema.spPost_Delete @PostId = "
                + postId.ToString()
                + ", @UserId="
                + this.User.FindFirst("UserId")?.Value;

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post!");
        }
    }
}
