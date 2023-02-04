﻿using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class DocumentMapDefinition : ContentMapDefinition<IContent, DocumentPropertyViewModel, DocumentVariantViewModel>, IMapDefinition
{
    public DocumentMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContent, DocumentViewModel>((_, _) => new DocumentViewModel(), Map);

    // Umbraco.Code.MapAll -Urls -TemplateKey
    private void Map(IContent source, DocumentViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.ContentTypeKey = source.ContentType.Key;
        target.Properties = MapPropertyViewModels(source);
        target.Variants = MapVariantViewModels(
            source,
            (culture, _, documentVariantViewModel) =>
            {
                documentVariantViewModel.State = GetSavedState(source, culture);
                documentVariantViewModel.PublishDate = culture == null
                    ? source.PublishDate
                    : source.GetPublishDate(culture);
            });
    }

    private ContentState GetSavedState(IContent content, string? culture)
    {
        if (content.Id <= 0 || (culture != null && content.IsCultureAvailable(culture) == false))
        {
            return ContentState.NotCreated;
        }

        var isDraft = content.PublishedState == PublishedState.Unpublished ||
                      (culture != null && content.IsCulturePublished(culture) == false);
        if (isDraft)
        {
            return ContentState.Draft;
        }

        var isEdited = culture != null
            ? content.IsCultureEdited(culture)
            : content.Edited;

        return isEdited ? ContentState.PublishedPendingChanges : ContentState.Published;
    }
}
