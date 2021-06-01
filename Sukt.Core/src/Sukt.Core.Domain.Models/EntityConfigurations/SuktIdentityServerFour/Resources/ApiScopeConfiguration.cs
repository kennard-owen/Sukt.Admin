﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sukt.Core.Domain.Models.IdentityServerFour;
using SuktCore.Shared;
using System;

namespace Sukt.Core.Domain.Models.SuktIdentityServerFour
{
    public class ApiScopeConfiguration : AggregateRootMappingConfiguration<ApiScope, Guid>
    {
        public override void Map(EntityTypeBuilder<ApiScope> b)
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Enabled).HasDefaultValue(true);
            b.Property(o => o.ShowInDiscoveryDocument).HasDefaultValue(true);
            b.ToTable("ApiScope");
        }
    }
}
