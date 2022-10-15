using System.Linq;
using DaxStudio.UI.Interfaces;
using AvalonDock.Layout;
using DaxStudio.UI.ViewModels;

namespace DaxStudio.UI.Utils
{
    public class DaxStudioLayoutStrategy : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
           var myViewModel = anchorableToShow.Content as IToolWindow;
            var doc = layout.Manager.DataContext as DocumentViewModel;
            if (doc != null && doc.IsLoadingLayout) return false;

            if (myViewModel != null)
            {
                
                var lap = layout.Descendents();
                var pane = lap.OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == myViewModel.DefaultDockingPane);
                // make sure that the ContentId property is populated so that the saving/loading of layouts works
                if(anchorableToShow.ContentId == null) anchorableToShow.ContentId = anchorableToShow.Content.GetType().ToString();
                if (pane != null)
                {
                    anchorableToShow.CanDockAsTabbedDocument = false;
                    pane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            return false;
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
            
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            // We only allow 1 document in the layout when using this layout strategy
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
            
        }

        public bool InsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
