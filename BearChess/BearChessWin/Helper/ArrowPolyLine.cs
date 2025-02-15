using System.Windows;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class ArrowPolyLine : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                                        typeof(PointCollection), typeof(ArrowPolyLine),
                                        new FrameworkPropertyMetadata(null,
                                                                      FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets a collection that contains the 
        ///     vertex points of the ArrowPolyline.
        /// </summary>
        public PointCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (PointCollection)GetValue(PointsProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of the ArrowPolyline class. 
        /// </summary>
        public ArrowPolyLine()
        {
            Points = new PointCollection();
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowPolyline.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                // Clear out the PathGeometry.
                pathgeo.Figures.Clear();

                // Try to avoid unnecessary indexing exceptions.
                if (Points.Count > 0)
                {
                    // Define a PathFigure containing the points.
                    pathfigLine.StartPoint = Points[0];
                    polysegLine.Points.Clear();

                    for (int i = 1; i < Points.Count; i++)
                        polysegLine.Points.Add(Points[i]);

                    pathgeo.Figures.Add(pathfigLine);
                }

                // Call the base property to add arrows on the ends.
                return base.DefiningGeometry;
            }
        }
    }
}