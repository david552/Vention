using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vention.Domain.Membership
{
    public static class MembershipRoleRules
    {
        public static bool IsOwnerOrAdmin(MembershipRole role)
            => role == MembershipRole.Owner || role ==  MembershipRole.Admin;

        public static bool IsAllowed(MembershipRole actual, IReadOnlyCollection<MembershipRole> allowedRoles)
            => allowedRoles.Contains(actual);
      
        public static bool CanAssign(MembershipRole actorRole, MembershipRole targetRole)
        {
            if (!IsOwnerOrAdmin(actorRole))
                return false;
            return targetRole >= actorRole;
        }
      
        public static bool CanRemove(MembershipRole actorRole, MembershipRole targetRole)
        {
            if (!IsOwnerOrAdmin(actorRole))
                return false;
            if (targetRole == MembershipRole.Owner && actorRole != MembershipRole.Owner)
                return false;
            return true;
        }
    }
}
