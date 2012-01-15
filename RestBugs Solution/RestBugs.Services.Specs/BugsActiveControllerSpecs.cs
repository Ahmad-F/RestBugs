using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Machine.Specifications;
using Moq;
using RestBugs.Services.Model;
using RestBugs.Services.Services;
using It = Machine.Specifications.It;

namespace RestBugs.Services.Specs
{
    public class when_getting_all_active_bugs
    {
        Establish context = () =>
        {
            var mockRepo = new Mock<IBugRepository>();
            mockRepo.Setup(r => r.GetAll()).Returns(BugHelper.TestBugList);
            controller = new BugsActiveController(mockRepo.Object);

            expectedResult = new List<Bug>
                             {
                                 new Bug {Status = BugStatus.Active, Priority = 1},
                                 new Bug {Status = BugStatus.Active, Priority = 1, Rank = 2},
                                 new Bug {Status = BugStatus.Active, Priority = 3}
                             };
        };

        Because of = () => { approvedBugs = controller.Get().Content.ReadAsync().Result; };

        It should_not_be_null = () => approvedBugs.ShouldNotBeNull();

        It should_return_3_bugs = () => approvedBugs.Count().ShouldEqual(3);

        It should_sort_bugs_in_order_of_priority_then_rank = () => approvedBugs.SequenceEqual(expectedResult);

        static IEnumerable<Bug> approvedBugs;
        static BugsActiveController controller;
        static IEnumerable<Bug> expectedResult;
    }

    public class when_posting_bug_to_active
    {
        Establish context = () =>
        {
            var testBug = new Bug { Id = 1 };
            var mockRepo = new Mock<IBugRepository>();
            mockRepo.Setup(r => r.Get(1)).Returns(testBug);
            mockRepo.Setup(r => r.GetAll()).Returns(new[] { testBug });

            controller = new BugsActiveController(mockRepo.Object);
        };

        Because of = () =>
        {
            var data = new JsonObject();
            data.Add("id", 1);
            data.Add("comments", "activating bug 1");

            result = controller.Post(data);
            resultContent = result.Content.ReadAsync().Result;
        };

        It should_not_be_null = () => result.ShouldNotBeNull();

        It should_have_1_bug_in_it = () => resultContent.Count().ShouldEqual(1);

        It should_contain_a_bug_with_id_1 = () => resultContent.First().Id.Equals(1);

        It should_add_comment_to_history = () => resultContent.First().History.Count.ShouldEqual(2);

        static BugsActiveController controller;
        static HttpResponseMessage<IEnumerable<Bug>> result;
        static IEnumerable<Bug> resultContent;
    }

    public class when_posting_nonexistant_bug_to_active
    {
        Establish context = () =>
        {
            var mockRepo = new Mock<IBugRepository>();
            mockRepo.Setup(r => r.Get(100)).Returns(null as Bug);
            controller = new BugsActiveController(mockRepo.Object);
        };

        Because of = () =>
        {
            dynamic data = new JsonObject();
            data.id = 100;
            data.comments = "activating bug 1";

            Exception = Catch.Exception(() => controller.Post(data));
        };

        It should_fail = () => Exception.ShouldNotBeNull();

        It should_be_http_exception = () => Exception.ShouldBeOfType(typeof(HttpResponseException));

        It should_throw_http_404 = () => ((HttpResponseException)Exception).Response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        static BugsActiveController controller;
        static Exception Exception;
    }
}