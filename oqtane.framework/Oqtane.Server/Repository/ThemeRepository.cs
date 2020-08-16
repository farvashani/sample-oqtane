﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Themes;

namespace Oqtane.Repository
{
    public class ThemeRepository : IThemeRepository
    {
        private List<Theme> _themes; // lazy load

        public IEnumerable<Theme> GetThemes()
        {
            return LoadThemes();
        }

        private List<Theme> LoadThemes()
        {
            if (_themes == null)
            {
                // get themes
                _themes = LoadThemesFromAssemblies();
            }
            return _themes;
        }

        private List<Theme> LoadThemesFromAssemblies()
        {
            List<Theme> themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                themes = LoadThemesFromAssembly(themes, assembly);
            }

            return themes;
        }

        private List<Theme> LoadThemesFromAssembly(List<Theme> themes, Assembly assembly)
        {
            Theme theme;
            List<Type> themeTypes = new List<Type>();

            Type[] themeControlTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IThemeControl))).ToArray();
            foreach (Type themeControlType in themeControlTypes)
            {
                // Check if type should be ignored
                if (themeControlType.IsOqtaneIgnore() || 
                    themeControlType.GetInterfaces().Contains(typeof(ILayoutControl)) || 
                    themeControlType.GetInterfaces().Contains(typeof(IContainerControl))) continue;

                // create namespace root typename
                string qualifiedThemeType = themeControlType.Namespace + ", " + themeControlType.Assembly.GetName().Name;

                int index = themes.FindIndex(item => item.ThemeName == qualifiedThemeType);
                if (index == -1)
                {
                    // Find all types in the assembly with the same namespace root
                    themeTypes = assembly.GetTypes()
                        .Where(item => !item.IsOqtaneIgnore())
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace == themeControlType.Namespace || item.Namespace.StartsWith(themeControlType.Namespace + "."))
                        .ToList();

                    // determine if this theme implements ITheme
                    Type themetype = themeTypes
                        .FirstOrDefault(item => item.GetInterfaces().Contains(typeof(ITheme)));
                    if (themetype != null)
                    {
                        var themeobject = Activator.CreateInstance(themetype) as ITheme;
                        theme = themeobject.Theme;
                    }
                    else
                    {
                        theme = new Theme
                        {
                            Name = themeControlType.Name,
                            Version = new Version(1, 0, 0).ToString()
                        };
                    }
                    // set internal properties
                    theme.ThemeName = qualifiedThemeType;
                    theme.Themes = new List<ThemeControl>();
                    theme.Layouts = new List<ThemeControl>();
                    theme.Containers = new List<ThemeControl>();
                    theme.AssemblyName = assembly.FullName.Split(",")[0];
                    themes.Add(theme);
                    index = themes.FindIndex(item => item.ThemeName == qualifiedThemeType);
                }
                theme = themes[index];

                var themecontrolobject = Activator.CreateInstance(themeControlType) as IThemeControl;
                theme.Themes.Add(
                    new ThemeControl
                    {
                        TypeName = themeControlType.FullName + ", " + themeControlType.Assembly.GetName().Name,
                        Name = theme.Name + " - " + ((string.IsNullOrEmpty(themecontrolobject.Name)) ? Utilities.GetTypeNameLastSegment(themeControlType.FullName, 0) : themecontrolobject.Name),
                        Thumbnail = themecontrolobject.Thumbnail,
                        Panes = themecontrolobject.Panes
                    }
                );

                // layouts
                Type[] layouttypes = themeTypes
                    .Where(item => item.GetInterfaces().Contains(typeof(ILayoutControl))).ToArray();
                foreach (Type layouttype in layouttypes)
                {
                    var layoutobject = Activator.CreateInstance(layouttype) as IThemeControl;
                    theme.Layouts.Add(
                        new ThemeControl
                        {
                            TypeName = layouttype.FullName + ", " + themeControlType.Assembly.GetName().Name,
                            Name = (string.IsNullOrEmpty(layoutobject.Name)) ? Utilities.GetTypeNameLastSegment(layouttype.FullName, 0) : layoutobject.Name,
                            Thumbnail = layoutobject.Thumbnail,
                            Panes = layoutobject.Panes
                        }
                    );
                }

                // containers
                Type[] containertypes = themeTypes
                    .Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                foreach (Type containertype in containertypes)
                {
                    var containerobject = Activator.CreateInstance(containertype) as IThemeControl;
                    theme.Containers.Add(
                        new ThemeControl
                        {
                            TypeName = containertype.FullName + ", " + themeControlType.Assembly.GetName().Name,
                            Name = (string.IsNullOrEmpty(containerobject.Name)) ? Utilities.GetTypeNameLastSegment(containertype.FullName, 0) : containerobject.Name,
                            Thumbnail = containerobject.Thumbnail,
                            Panes = ""
                        }
                    );
                }

                themes[index] = theme;
            }
            return themes;
        }
    }
}
