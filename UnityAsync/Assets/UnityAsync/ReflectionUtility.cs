using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace UnityAsync
{
	internal static class ReflectionUtility
	{
		public static void GenerateFieldGetter<TTarget, TField>(string field, out Func<TTarget, TField> getter)
		{
			if(!TryGenerateFieldGetter(field, out getter))
			{
				Debug.LogError
				(
					$"Unable to create field getter for {typeof(TTarget).Name}.{field}; internal implementation may have changed."
				);
			}
		}
		
		public static void GenerateFieldSetter<TTarget, TField>(string field, out Action<TTarget, TField> setter)
		{
			if(!TryGenerateFieldSetter(field, out setter))
			{
				Debug.LogError
				(
					$"Unable to create field setter for {typeof(TTarget).Name}.{field}; internal implementation may have changed."
				);
			}
		}
		
		public static bool TryGenerateFieldGetter<TOwner, TField>(string fieldName, out Func<TOwner, TField> getter)
		{
			try
			{
				var tOwner = typeof(TOwner);
				
				var field = tOwner.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				
				var targetExp = Expression.Parameter(tOwner);
				var member = Expression.Field(targetExp, field);
				
				getter = Expression.Lambda<Func<TOwner, TField>>(member, targetExp).Compile();
				
				return true;
			}
			catch
			{
				getter = null;
				
				return false;
			}
		}
		
		public static bool TryGenerateFieldSetter<TOwner, TField>(string fieldName, out Action<TOwner, TField> setter)
		{
			try
			{
				var tOwner = typeof(TOwner);
				var tField = typeof(TField);
				
				var field = tOwner.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				
				var targetParam = Expression.Parameter(tOwner);
				var valueParam = Expression.Parameter(tField);
				var member = Expression.Field(targetParam, field);
				var exp = Expression.Assign(member, valueParam);
				
				setter = Expression.Lambda<Action<TOwner, TField>>(exp, targetParam, valueParam).Compile();
				
				return true;
			}
			catch(Exception e)
			{
				setter = null;
				
				Debug.LogException(e);
				
				return false;
			}
		}
	}
}