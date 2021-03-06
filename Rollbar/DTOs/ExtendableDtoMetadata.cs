﻿namespace Rollbar.DTOs
{
    using Rollbar.Diagnostics;
    using Rollbar.Utils;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class ExtendableDtoMetadata
    {
        public Type ExtendableDtoType { get; private set; }

        public IReadOnlyDictionary<string, PropertyInfo> ReservedPropertyInfoByReservedKey { get; private set; }

        public static IReadOnlyDictionary<Type, ExtendableDtoMetadata> BuildAll()
        {
            Type[] derivedTypes = 
                ReflectionUtil.GetSubClassesOf(typeof(ExtendableDtoBase));

            Dictionary<Type, ExtendableDtoMetadata> result = 
                new Dictionary<Type, ExtendableDtoMetadata>(derivedTypes.Length);

            foreach(var type in derivedTypes)
            {
                result.Add(type, Build(type));
            }

            return result;
        }

        private static ExtendableDtoMetadata Build(Type extendableDtoType)
        {
            ExtendableDtoMetadata result = new ExtendableDtoMetadata();
            result.ExtendableDtoType = extendableDtoType;

            List<Type> reservedPropertiesNestedTypes = new List<Type>();
            Type extendableDtoHierarchyType = extendableDtoType;
            while (extendableDtoHierarchyType != null)
            {
                Type reservedPropertiesNestedType = ReflectionUtil.GetNestedTypeByName(
                    extendableDtoHierarchyType,
                    ExtendableDtoBase.reservedPropertiesNestedTypeName,
                    BindingFlags.Public | BindingFlags.Static
                    );
                if (reservedPropertiesNestedType != null)
                {
                    reservedPropertiesNestedTypes.Add(reservedPropertiesNestedType);
                }
                if (extendableDtoHierarchyType.BaseType == typeof(ExtendableDtoBase))
                {
                    break;
                }
                if (extendableDtoHierarchyType.BaseType != null)
                {
                    extendableDtoHierarchyType = extendableDtoHierarchyType.BaseType;
                }
            }
            //Assumption.AssertTrue(reservedPropertiesNestedTypes.Count > 0, nameof(reservedPropertiesNestedTypes.Count));

            List<FieldInfo> reservedAttributes = new List<FieldInfo>();
            foreach (Type reservedPropertiesNestedType in reservedPropertiesNestedTypes)
            {
                reservedAttributes.AddRange(ReflectionUtil.GetAllPublicStaticFields(reservedPropertiesNestedType));
            }
            //Assumption.AssertTrue(reservedAttributes.Count > 0, nameof(reservedAttributes.Count));

            //Type reservedPropertiesNestedType = ReflectionUtil.GetNestedTypeByName(
            //    extendableDtoType,
            //    ExtendableDtoBase.reservedPropertiesNestedTypeName,
            //    BindingFlags.Public | BindingFlags.Static
            //    );
            //Assumption.AssertNotNull(reservedPropertiesNestedType, nameof(reservedPropertiesNestedType));

            //var reservedAttributes =
            //    ReflectionUtil.GetAllPublicStaticFields(reservedPropertiesNestedType);

            Dictionary<string, PropertyInfo> reservedPropertyInfoByName = 
                new Dictionary<string, PropertyInfo>(reservedAttributes.Count);
            result.ReservedPropertyInfoByReservedKey = reservedPropertyInfoByName;

            foreach(var reservedAttribue in reservedAttributes)
            {
                var property = 
                    extendableDtoType.GetProperty(reservedAttribue.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                Assumption.AssertNotNull(property, nameof(property));

                string reservedKey = ReflectionUtil.GetStaticFieldValue<string>(reservedAttribue);
                Assumption.AssertNotNullOrWhiteSpace(reservedKey, nameof(reservedKey));

                reservedPropertyInfoByName.Add(reservedKey, property);
            }

            return result;
        }
    }
}
