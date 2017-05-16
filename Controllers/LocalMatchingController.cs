using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using local_matching_service.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace local_matching_service.Controllers
{
    [Route("local-matching")]
    public class LocalMatchingController : Controller
    {
        [HttpPost("match")]
        public dynamic Match([FromBody] dynamic request)
        {
            var users = Database.getUsers();

            // The local matching service runs cycle 0:

            // The local matching service tries to find a match between the user’s hashed PID and a hashed PID in the local matching datastore.
            if (null != users.FirstOrDefault(user => user.verifyHashedPid == request?.hashedPid?.ToString())) {
                // The local matching service sends a match response to the MSA and forwards it and the hashed PID to the government service via the GOV.UK Verify hub.
                return new { result = "match" };
            }

            // The local matching service runs cycle 1:

            // The local matching service tries to find a match between the user’s matching dataset and a record in government service data sources.
            var cycle1Match = users.Where(user => {
                return user.firstname == request?.matchingDataset?.firstName?.value.ToString()
                  && user.lastname == request?.matchingDataset?.surnames[0]?.value.ToString()
                  && user.dateOfBirth == request?.matchingDataset?.dateOfBirth?.value.ToString();
            }).ToList();

            if (cycle1Match.Count == 1) {
                // The local matching service saves the hashed PID in a datastore along with the user’s record. Future matches with cycle 0 will use this data when the same user returns, having been verified by the same identity provider.
                Database.setHashedPid(cycle1Match[0].id, request?.hashedPid?.ToString());
                // The local matching service sends a match response to the MSA and forwards it and the hashed PID to the government service via the GOV.UK Verify hub.
                return new { result = "match" };
            }

            if (cycle1Match.Count == 0) {
                return new { result = "no-match" };
            }

            if (cycle1Match.Count > 1 && request?.cycle3Dataset != null) {
                // The local matching service tries to find a match between the user’s additinal information and a record in government service data sources.
                var cycle3Match = cycle1Match.FirstOrDefault(x => {
                    return request?.cycle3Dataset?.attributes?.nino?.ToString() == x.nationalInsuranceNumber;
                });
                if (cycle3Match != null) {
                    // The local matching service saves the hashed PID in a datastore along with the user’s record. Future matches with cycle 0 will use this data when the same user returns, having been verified by the same identity provider.
                    Database.setHashedPid(cycle3Match.id, request?.hashedPid?.ToString());
                    // The local matching service sends a match response to the MSA, which forwards it and the hashed PID to the government service via the GOV.UK Verify hub.
                    return new { result = "match" };
                }
            }
            return new { result = "no-match" };
        }
    }
}
