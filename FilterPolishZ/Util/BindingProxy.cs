using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilterPolishZ.Util
{
    /// <summary>
    /// Provides a convenient method for objects outside of the visual tree
    /// (such as context menus) to access a bound datacontext
    /// </summary>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// The property to bind a datacontext too
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                "Data",
                typeof(object),
                typeof(BindingProxy));

        /// <summary>
        /// The datacontext that we wish to expose
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>
        /// Creates a new instance of the Freezable derived class
        /// </summary>
        /// <returns>A new instance of the BindingProxy class</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}