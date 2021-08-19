﻿using Microsoft.AspNetCore.Http;
using Routine.Core.Rest;
using Routine.Service.RequestHandlers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Routine.Service
{
    public class RoutineMiddleware
	{
		private readonly RequestDelegate next;

		private readonly string rootPath;
		private readonly IndexRequestHandler indexHandler;
		private readonly FileRequestHandler fileHandler;
		private readonly FontsRequestHandler fontsHandler;
		private readonly ConfigurationRequestHandler configurationHandler;
		private readonly ApplicationModelRequestHandler applicationModelHandler;
		private readonly HandleRequestHandler handleHandler;

		public RoutineMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, IJsonSerializer jsonSerializer, IServiceContext serviceContext)
		{
			this.next = next;

			rootPath = serviceContext.ServiceConfiguration.GetPath();
			indexHandler = new IndexRequestHandler(serviceContext, jsonSerializer, httpContextAccessor);
			fileHandler = new FileRequestHandler(serviceContext, jsonSerializer, httpContextAccessor);
			fontsHandler = new FontsRequestHandler(serviceContext, jsonSerializer, httpContextAccessor);
			configurationHandler = new ConfigurationRequestHandler(serviceContext, jsonSerializer, httpContextAccessor);
			applicationModelHandler = new ApplicationModelRequestHandler(serviceContext, jsonSerializer, httpContextAccessor);
			handleHandler = new HandleRequestHandler(serviceContext, jsonSerializer, httpContextAccessor,
				actionFactory: resolution => resolution.HasOperation
					? new DoRequestHandler(serviceContext, jsonSerializer, httpContextAccessor, resolution)
					: new GetRequestHandler(serviceContext, jsonSerializer, httpContextAccessor, resolution)
			);
		}

		public async Task Invoke(HttpContext context)
		{
			var path = $"{context.Request.Path}".ToLowerInvariant();

			if (path == "/")
			{
				await indexHandler.WriteResponse();
			}
			else if (path == $"/{rootPath}file")
			{
				await fileHandler.WriteResponse();
			}
			else if (Regex.IsMatch(path, $"/{rootPath}fonts/[^/]*/f"))
			{
				await fontsHandler.WriteResponse();
			}
			else if (path == $"/{rootPath}configuration")
			{
				await configurationHandler.WriteResponse();
			}
			else if (path == $"/{rootPath}applicationmodel")
			{
				await applicationModelHandler.WriteResponse();
			}
			else if (path.StartsWith($"/{rootPath}"))
			{
				await handleHandler.WriteResponse();
			}
			else
			{
				await next(context);
			}
		}
	}
}