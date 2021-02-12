//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using OrchardCore.DisplayManagement.Descriptors;

//namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
//{
//    public class UserEditShapes : IShapeTableProvider
//    {
//        private readonly IAuthorizationService _authorizationService;
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public UserEditShapes(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//            _authorizationService = authorizationService;
//        }
//        public void Discover(ShapeTableBuilder builder)
//        {
//            builder.Describe("UserFields_Edit").OnDisplaying(action =>
//            {
                
//                var user = _httpContextAccessor.HttpContext?.User;
//                if (user != null && !user.IsInRole("Administrator"))
//                {
//#pragma warning disable S1481 // Unused local variables should be removed
//                    var value = action.DisplayContext.Value;
//#pragma warning restore S1481 // Unused local variables should be removed

//#pragma warning disable S3626 // Jump statements should not be redundant
//                    return;
//#pragma warning restore S3626 // Jump statements should not be redundant
//                }
               
//            });
//        }
//    }
//}
