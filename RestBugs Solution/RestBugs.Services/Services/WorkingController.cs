﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using RestBugs.Services.Model;

namespace RestBugs.Services.Services
{
    public class WorkingController : ApiController
    {
        readonly IBugRepository _bugRepository;

        public WorkingController(IBugRepository bugRepository) {
            _bugRepository = bugRepository;
        }

        private IEnumerable<BugDTO> GetDtos()
        {
            var bugs = _bugRepository.GetAll()
                .Where(b => b.Status == BugStatus.Working)
                .OrderBy(b => b.Priority)
                .ThenBy(b => b.Rank);

            var dtos = Mapper.Map<IEnumerable<Bug>, IEnumerable<BugDTO>>(bugs);

            return dtos;
        }

        public HttpResponseMessage<IEnumerable<BugDTO>>Get() {
            var response = new HttpResponseMessage<IEnumerable<BugDTO>>(GetDtos());
            return response;
        }

        public HttpResponseMessage<IEnumerable<BugDTO>> Post(int id, string comments)
        {
            Bug bug = _bugRepository.Get(id); //bugId
            if (bug == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            bug.Activate(comments);

            var response = new HttpResponseMessage<IEnumerable<BugDTO>>(GetDtos()) { StatusCode = HttpStatusCode.OK };
            
            return response;
        }
    }
}