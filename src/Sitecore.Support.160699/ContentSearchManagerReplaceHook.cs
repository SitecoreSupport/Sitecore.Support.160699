using Sitecore.ContentSearch.Utilities;
using System;
using System.Collections.Generic;
using Sitecore.ContentSearch;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;

namespace Sitecore.Support
{
    public class ContentSearchManagerReplaceHook : Sitecore.Events.Hooks.IHook
    {
        public void Initialize()
        {
            var locator = Sitecore.ContentSearch.ContentSearchManager.Locator;
            locator.Register<QueryBuilder>(inst => new CustomQueryBuilder());
        }

        public class CustomQueryBuilder : QueryBuilder
        {
            public CustomQueryBuilder()
            {
                Log.Info("[Support] CustomQueryBuilder is in place", this);
            }


            protected override Type ResolveFieldTypeByName(string fieldName, IProviderSearchContext context)
            {

                if (context == null)
                {
                    return null;
                }
                if (context.Index == null)
                {
                    return null;
                }
                if (context.Index.Configuration == null)
                {
                    return null;
                }
                IFieldMap fieldMap = context.Index.Configuration.FieldMap;
                if (fieldMap == null)
                {
                    return null;
                }
                AbstractSearchFieldConfiguration abstractSearchFieldConfiguration = fieldMap.GetFieldConfiguration(fieldName);
                var database = (Context.ContentDatabase ?? Context.Database);
                if (abstractSearchFieldConfiguration == null && database != null)
                {
                    string query = string.Format("//*[@@templateid='{0}' and @@key='{1}']", TemplateIDs.TemplateField, fieldName);
                    Item item = database.SelectSingleItem(query);
                    if (item == null)
                    {
                        IFieldMapEx fieldMapEx = fieldMap as IFieldMapEx;
                        if (fieldMapEx != null)
                        {
                            fieldMapEx.AddFieldByFieldName(fieldName, null, new Dictionary<string, string>(), null);
                        }
                    }
                    else
                    {
                        Field field = item.Fields["Type"];
                        string text = (field != null) ? field.Value.ToLower() : null;
                        if (text != null)
                        {
                            abstractSearchFieldConfiguration = fieldMap.GetFieldConfigurationByFieldTypeName(text);
                        }
                    }
                }
                if (abstractSearchFieldConfiguration == null)
                {
                    return null;
                }
                string text2;
                if (!abstractSearchFieldConfiguration.Attributes.TryGetValue("type", out text2))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(text2))
                {
                    return null;
                }
                return Type.GetType(text2, false);
            }
        }

    }
}
