using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Text.RegularExpressions;

namespace DevBin {
    public class RegexConstraint : IRouteConstraint {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {
            Console.WriteLine(httpContext.Request.Path);
            return Regex.IsMatch(httpContext.Request.Path, @"^[A-Za-z]{8}$");
        }
    }
}
