﻿using JetBrains.Annotations;

using System;
using System.Linq;

namespace Intersect.Client.Framework.Content
{

    public static class ContentTypesExtensions
    {

        [NotNull]
        public static Type GetAssetType(this ContentTypes contentType)
        {
			string memberName = contentType.ToString();
			System.Reflection.MemberInfo memberInfo = typeof(ContentTypes).GetMember(memberName).FirstOrDefault();
            if (memberInfo == null)
            {
                throw new InvalidOperationException($@"{nameof(ContentTypes)} missing expected member: {memberName}");
            }

			object attribute = memberInfo.GetCustomAttributes(typeof(AssetTypeAttribute), true).FirstOrDefault();
            if (attribute is AssetTypeAttribute assetTypeAttribute)
            {
                return assetTypeAttribute.Type;
            }

            throw new InvalidOperationException(
                $@"{nameof(ContentTypes)} missing {nameof(AssetTypeAttribute)} on member: {memberName}"
            );
        }

    }

}
