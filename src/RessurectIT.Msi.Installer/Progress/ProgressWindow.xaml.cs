using System.Windows;
using DryIocAttributes;
using Microsoft.Extensions.Logging;

namespace RessurectIT.Msi.Installer.Progress
{
    /// <summary>
    /// Used for displaying progress window
    /// </summary>
    [ExportEx(typeof(IProgressWindow))]
    [CurrentScopeReuse]
    public partial class ProgressWindow : IProgressWindow
    {
        #region private fields

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<ProgressWindow> _logger;
        #endregion


        #region dependency properties

        /// <summary>
        /// Dependency property for <see cref="Message"/>
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message",
                                                                                                typeof(string),
                                                                                                typeof(ProgressWindow),
                                                                                                new PropertyMetadata(default(string)));

        /// <summary>
        /// Dependency property for <see cref="TitleText"/>
        /// </summary>
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText",
                                                                                                  typeof(string),
                                                                                                  typeof(ProgressWindow),
                                                                                                  new PropertyMetadata(default(string)));
        #endregion


        #region public properties

        /// <summary>
        /// Gets or sets message value to be displayed
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Gets or sets text for title
        /// </summary>
        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }
        #endregion
        

        #region constructors

        /// <summary>
        /// Creates instance of <see cref="ProgressWindow"/>
        /// </summary>
        /// <param name="logger">Logger used for logging</param>
        public ProgressWindow(ILogger<ProgressWindow> logger)
        {
            _logger = logger;

            InitializeComponent();
        }
        #endregion
        

        #region public methods - Implementation of IDisposable

        /// <inheritdoc />
        public void ShowProgressMessage(string message, string updateId)
        {
            Visibility = Visibility.Visible;
            Show();

            _logger.LogDebug("Displaying message '{message}' in progress window for update '{updateId}'", message, updateId);

            TitleText = $"Updating {updateId}";
            Message = message;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _logger.LogDebug("Disposing progress window");

            Visibility = Visibility.Hidden;
            Close();
        }
        #endregion
    }
}
