﻿using Moq;
using Routine.Core.Rest;
using System;
using System.Linq.Expressions;
using System.Net;

namespace Routine.Test.Service.Stubs
{
    public class Async : IRestClientStubber
    {
        public void SetUpPost(Mock<IRestClient> mock,
            string url,
            Expression<Func<RestRequest, bool>> match,
            RestResponse response
        ) => mock.Setup(rc => rc.PostAsync(url, It.Is(match))).ReturnsAsync(response);

        public void SetUpPost(Mock<IRestClient> mock,
            string url,
            Expression<Func<RestRequest, bool>> match,
            WebException exception
        ) => mock.Setup(rc => rc.PostAsync(url, It.Is(match))).ThrowsAsync(exception);

        public void VerifyPost(Mock<IRestClient> mock, Expression<Func<RestRequest, bool>> match) =>
            mock.Verify(rc => rc.PostAsync(It.IsAny<string>(), It.Is(match)));
    }
}