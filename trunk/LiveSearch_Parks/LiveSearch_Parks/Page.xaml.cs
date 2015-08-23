using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using LiveSearch_Parks.MSN_SearchLiveService;


namespace LiveSearch_Parks
{
	public partial class Page : UserControl
	{

		public Page()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DoSearch();
        }


        // put this in after the Page : UserControl declaration
        // these constants determine the State for keyword search, and center position of the map.
        private const string _searchState = "California";
        private const double _searchStateLatitude = 37.387617;
        private const double _searchStateLongitude = -119.893799;

        // put this in after the Page { } constructor
        private string GetSearchString()
        {
            string searchString = _searchState + " parks ";

            if (cbxBike.IsChecked.Value == true)
                searchString += "bike ";

            if (cbxCamp.IsChecked.Value == true)
                searchString += "camp ";

            if (cbxBoat.IsChecked.Value == true)
                searchString += "boat ";

            if (cbxHike.IsChecked.Value == true)
                searchString += "hike ";

            if (txtTown.Text.Trim().Length > 0)
                searchString += txtTown.Text;

            return searchString;
        }

        private void DoSearch()
        {
                sbBusy.Begin();

            // initiate a Live Search request
            MSN_SearchLiveService.MSNSearchPortTypeClient s = new MSNSearchPortTypeClient();
            s.SearchCompleted += new EventHandler<SearchCompletedEventArgs>(s_SearchCompleted);
            SearchRequest searchRequest = new SearchRequest();
            int arraySize = 2;
            SourceRequest[] sr = new SourceRequest[arraySize];

            if (cbxWebPages.IsChecked.Value)
            {
                sr[0] = new SourceRequest();
                sr[0].Source = SourceType.Web;
            }
            if (cbxImages.IsChecked.Value)
            {
                sr[1] = new SourceRequest();
                sr[1].Source = SourceType.Image;
                sr[1].ResultFields = ResultFieldMask.Image | ResultFieldMask.Url;
            }

            searchRequest.Query = GetSearchString();
            searchRequest.Requests = sr;
            searchRequest.AppID = App._MSNSearchAppId;
            searchRequest.CultureInfo = "en-US";
            s.SearchAsync(searchRequest);
        }

        /// <summary>
        /// Defines a search result from Web Page, Image, or Map (used for binding)
        /// </summary>
        public class SearchResult
        {
            public int HitNumber { get; set; }
            public string ResultUrl { get; set; }
            public string ImageUrl { get; set; }
            public string Description { get; set; }
            public double ImageHeight { get; set; }
            public double ImageWidth { get; set; }
        }

        void s_SearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            // hide the busy status
            sbBusy.Stop();

            // clear out any existing hits
            int totalHits = 0;
            lstResults.ItemsSource = null;
            stkImages.Children.Clear();

            List<SearchResult> lstWebPages = new List<SearchResult>();

            if (e.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert("There was a problem contacting the Live Search Server:\n\n" + e.Error.Message + "\n\nUsing Test images only.");
                return;
            }

            foreach (SourceResponse sourceResponse in e.Result.Responses)
            {
                Result[] sourceResults = sourceResponse.Results;
                if (sourceResponse.Total > 0)
                {
                    foreach (Result x in sourceResults)
                    {
                        if (x.Image != null && cbxImages.IsChecked.Value)
                        {
                            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                            img.Height = 100;
                            img.Margin = new Thickness(5, 0, 5, 0);
                            Uri ImageUri = new Uri(x.Image.ImageURL, UriKind.Absolute);
                            ImageSource imgSrc = new System.Windows.Media.Imaging.BitmapImage(ImageUri);
                            img.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(img_ImageFailed);
                            img.SetValue(System.Windows.Controls.Image.SourceProperty, imgSrc);
                            stkImages.Children.Add(img);
                            totalHits += 1;
                        }
                        else
                            if (cbxWebPages.IsChecked.Value)
                            {
                                // web page hit
                                SearchResult result = new SearchResult();
                                result.ResultUrl = x.Url;
                                result.Description = x.Description;
                                lstWebPages.Add(result);
                                totalHits += 1;
                            }
                    }
                }

                if (lstResults.Items.Count > 0)
                    lstResults.SelectedIndex = 0;

                if (totalHits == 0)
                    System.Windows.Browser.HtmlPage.Window.Alert("There were no Web Page or Image results found for your query.");

            }

            lstResults.ItemsSource = lstWebPages;

            //A function to add when you do Virtual Earth
            DoMapSearch();

        }

        void img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // by handling this event, we avoid errors when images are not found on web
        }


        // TODO: Enter DoMapSearch code below!
        private void DoMapSearch()
        {
            // initiates a Virtual Earth Keyword type search

            // show the busy status
            sbBusy.Begin();

            VirtualEarthSearchService.SearchRequest searchRequest = new VirtualEarthSearchService.SearchRequest();
            lstResultsMap.ItemsSource = null;
            imgMap.Source = null;

            // Set the credentials using a valid Virtual Earth token
            searchRequest.Credentials = new VirtualEarthSearchService.Credentials();
            searchRequest.Credentials.Token = App._virtualEarthToken;

            VirtualEarthSearchService.StructuredSearchQuery ssQuery = new VirtualEarthSearchService.StructuredSearchQuery();
            ssQuery.Keyword = GetSearchString();
            if (txtTown.Text.Trim().Length > 0)
                ssQuery.Location = txtTown.Text + ", ";
            ssQuery.Location += _searchState;
            searchRequest.StructuredQuery = ssQuery;
            searchRequest.SearchOptions = new VirtualEarthSearchService.SearchOptions();
            searchRequest.SearchOptions.Count = 9;

            // Make the search request 
            VirtualEarthSearchService.SearchServiceClient searchService = new VirtualEarthSearchService.SearchServiceClient();
            searchService.SearchCompleted += new EventHandler<LiveSearch_Parks.VirtualEarthSearchService.SearchCompletedEventArgs>(MapSearch_SearchCompleted);
            searchService.SearchAsync(searchRequest);

        }

        void MapSearch_SearchCompleted(object sender, LiveSearch_Parks.VirtualEarthSearchService.SearchCompletedEventArgs e)
        {
            // The result is a SearchResponse Object
            VirtualEarthSearchService.SearchResponse searchResponse = e.Result;

            List<SearchResult> lstMapResults = new List<SearchResult>();
            cnvMap.Children.Clear();

            if (searchResponse.ResultSets[0].Results.Count > 0)
            {

                // request a map image for these hits with pushpins
                ImageryService.MapUriRequest mapUriRequest = new ImageryService.MapUriRequest();

                lstResultsMap.ItemsSource = null;

                // Set credentials using a valid Virtual Earth Token
                mapUriRequest.Credentials = new ImageryService.Credentials();
                mapUriRequest.Credentials.Token = App._virtualEarthToken;

                // Set the map style and zoom level
                ImageryService.MapUriOptions mapUriOptions = new ImageryService.MapUriOptions();
                mapUriOptions.Style = ImageryService.MapStyle.Road; // ImageryService.MapStyle.AerialWithLabels;
                mapUriOptions.ZoomLevel = 5;

                // Set the size of the requested image to match the size of the image control
                mapUriOptions.ImageSize = new ImageryService.SizeOfint();
                mapUriOptions.ImageSize.Height = (int)imgMap.Height;
                mapUriOptions.ImageSize.Width = (int)imgMap.Width;

                mapUriRequest.Options = mapUriOptions;
                int totalHits = 0;
                foreach (LiveSearch_Parks.VirtualEarthSearchService.BusinessSearchResult result in searchResponse.ResultSets[0].Results)
                {
                    SearchResult searchHit = new SearchResult();
                    searchHit.HitNumber = totalHits + 1;
                    searchHit.Description = result.Name;
                    if (result.Website != null)
                        searchHit.ResultUrl = result.Website.ToString();

                    lstMapResults.Add(searchHit);

                    // add the client side pushpin
                    ucPushPin ucPushPinNew = new ucPushPin();
                    Point ptPin = LatLongToPixelXY(_searchStateLatitude, _searchStateLongitude, result.LocationData.Locations[0].Latitude, result.LocationData.Locations[0].Longitude, mapUriOptions.ZoomLevel.Value);
                    if (ptPin.X < cnvMap.Width - ucPushPinNew.Width && ptPin.Y < cnvMap.Height - ucPushPinNew.Height)
                    {
                        ucPushPinNew.SetValue(Canvas.LeftProperty, Convert.ToDouble(ptPin.X));
                        ucPushPinNew.SetValue(Canvas.TopProperty, Convert.ToDouble(ptPin.Y));
                        ucPushPinNew.SetValue(Canvas.ZIndexProperty, 1000);
                        if (searchHit.ResultUrl != null)
                        {
                            ucPushPinNew.lnkUrl.NavigateUri = new Uri(searchHit.ResultUrl, UriKind.Absolute);
                            ucPushPinNew.lnkUrl.Content = searchHit.ResultUrl;
                        }
                        ucPushPinNew.txtDescription.Text = searchHit.Description;
                        cnvMap.Children.Add(ucPushPinNew);
                    }

                    totalHits++;
                }


                // Set the location of the requested image
                mapUriRequest.Center = new ImageryService.Location();
                mapUriRequest.Center.Latitude = _searchStateLatitude;
                mapUriRequest.Center.Longitude = _searchStateLongitude;

                ImageryService.ImageryServiceClient imageryService = new ImageryService.ImageryServiceClient();
                imageryService.GetMapUriCompleted += new EventHandler<LiveSearch_Parks.ImageryService.GetMapUriCompletedEventArgs>(imageryService_GetMapUriCompleted);
                imageryService.GetMapUriAsync(mapUriRequest);

                lstResultsMap.ItemsSource = lstMapResults;
            }
            else
                System.Windows.Browser.HtmlPage.Window.Alert("There were no Map results found for your query.");

            // hide the busy status
            sbBusy.Stop();

        }



        void imageryService_GetMapUriCompleted(object sender, LiveSearch_Parks.ImageryService.GetMapUriCompletedEventArgs e)
        {
            // this is the callback handler, executed when the GetMapUri asynchronous call completes

            // The result is an MapUriResponse Object
            ImageryService.MapUriResponse mapUriResponse = e.Result;

            //Note that we have to replace the {token} value in the image Uri
            BitmapImage bmpImg = new BitmapImage(new Uri(
              mapUriResponse.Uri.Replace("{token}", App._virtualEarthToken)));
            imgMap.Source = bmpImg;

        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="latitude">Latitude of the point, in degrees.</param>
        /// <param name="longitude">Longitude of the point, in degrees.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>
        /// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>
        public Point LatLongToPixelXY(double centerLatitude, double centerLongitude, double latitude, double longitude, int levelOfDetail)
        {
            double metersPerPixel = Math.Abs(156543.04 * Math.Cos(centerLatitude) / (Math.Pow(2, levelOfDetail + 1)));

            double x = centerLongitude - (MetersToDecimalDegrees(metersPerPixel * imgMap.Width / 2));
            double y = centerLatitude + (MetersToDecimalDegrees(metersPerPixel * imgMap.Height / 2));
            double width = MetersToDecimalDegrees(metersPerPixel * (imgMap.Width));
            double height = MetersToDecimalDegrees(metersPerPixel * (imgMap.Height));
            return ToPagePoint(longitude, latitude, x, y, width, height, imgMap);
        }

        /// <summary>
        /// Converts lat/long position on a map to a pixel x,y position on the screen
        /// </summary>
        /// <param name="mapX">The longitude</param>
        /// <param name="mapY">The latitude</param>
        /// <returns></returns>
        public static Point ToPagePoint(double mapX, double mapY, double mapLeft, double mapTop, double mapWidth, double mapHeight, System.Windows.Controls.Image mapImage)
        {
            double diffX, diffY;

            diffX = (mapX - mapLeft);
            diffY = (mapTop - mapY);

            double iX = (diffX / mapWidth) * mapImage.Width;
            double iY = (diffY / mapHeight) * mapImage.Height;

            return new Point(iX, iY);
        }

        /// <summary>
        /// Gets a value in decimal degrees equal to the number of meters passed in
        /// </summary>
        public static double MetersToDecimalDegrees(double meters)
        {
            double degrees = meters * .00001 / 0.676656;

            return degrees;
        }
	}
}