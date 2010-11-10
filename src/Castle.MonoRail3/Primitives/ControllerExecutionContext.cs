namespace Castle.MonoRail3.Primitives
{
	using System;
	using System.Web;
	using System.Web.Routing;

	public class ControllerExecutionContext
	{
		internal ControllerExecutionContext(HttpContextBase httpContext, 
			object controller, RouteData data, 
			ControllerDescriptor controllerDescriptor)
		{
			HttpContext = httpContext;
			Controller = controller;
			RouteData = data;
			ControllerDescriptor = controllerDescriptor;
		}

		// readonly
		public object Controller { get; private set; }
		public HttpContextBase HttpContext { get; private set; }
		public RouteData RouteData { get; private set; }
		public ControllerDescriptor ControllerDescriptor { get; private set; }

		public object InvocationResult { get; set; }

		public ActionDescriptor SelectedAction { get; set; }
	}
}