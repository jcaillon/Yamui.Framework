﻿#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (GenericThemeHolder.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yamui.Framework.Helper {
    /// <summary>
    /// A class that can be inherited to hold themes,
    /// instead of directly carrying Color fields, it only has a dictionary of property -> value
    /// that is used to fill the Color properties of the class that inherits from this one
    /// </summary>
    public class GenericThemeHolder {

        #region fields

        /// <summary>
        /// Theme's name
        /// </summary>
        public string ThemeName = "";

        /// <summary>
        /// a property -> value dictionary
        /// </summary>
        public Dictionary<string, string> SavedStringValues = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Utilities

        /// <summary>
        /// Allows to read a given file as a theme file and returns the list of themes read from it
        /// </summary>
        public static List<T> ReadThemeFile<T>(string filePath, byte[] dataResources, Encoding encoding) where T : GenericThemeHolder {
            // the dico below will contain key -> values of the theme
            var listOfThemes = new List<T>();
            GenericThemeHolder curTheme = null;
            Dictionary<string, string> previousStringValues = new Dictionary<string, string>();

            Utilities.ForEachLine(filePath, dataResources,
                (i, line) => {
                    // beginning of a new theme, read its name
                    if (line.Length > 2 && line[0] == '>') {
                        if (curTheme != null)
                            previousStringValues = curTheme.SavedStringValues;
                        curTheme = Activator.CreateInstance<T>();
                        curTheme.ThemeName = line.Substring(2).Trim();
                        curTheme.SavedStringValues = new Dictionary<string, string>(previousStringValues);
                        listOfThemes.Add((T) curTheme);
                    } else if (curTheme == null)
                        return;

                    // fill the theme's dico
                    var pos = line.IndexOf('\t');
                    if (pos >= 0)
                        curTheme.SetStringValues(line.Substring(0, pos).Trim(), line.Substring(pos + 1).Trim());
                },
                Encoding.Default,
                exception => {
                    // rethrow the same exception to the calling method
                    throw new Exception(exception.ToString());
                });

            return listOfThemes;
        }

        /// <summary>
        /// Allows to replace all the occurrences of @color by the actual color using the internal dico
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string ReplaceAliasesByColor(string source) {
            // try to replace a variable name by it's html color value
            var regex = new Regex(@"@([a-zA-Z]*)", RegexOptions.IgnoreCase);
            return regex.Replace(source, match => {
                if (SavedStringValues.ContainsKey(match.Groups[1].Value))
                    return GetHtmlColor(SavedStringValues[match.Groups[1].Value]);
                throw new Exception("Couldn't find the color " + match.Groups[1].Value + "!");
            });
        }

        #endregion

        #region Core

        /// <summary>
        /// Saves info on the dictionary, allows to recompute the Color values later
        /// </summary>
        public void SetStringValues(string name, string value) {
            if (!SavedStringValues.ContainsKey(name))
                SavedStringValues.Add(name, value);
            else
                SavedStringValues[name] = value;
        }

        /// <summary>
        /// Set the values of this instance, using a dictionary of key -> values
        /// </summary>
        public void SetColorValues(Type thisType) {
            if (SavedStringValues == null)
                return;

            // for each field of this object, try to assign its value with the _savedStringValues dico
            foreach (var fieldInfo in thisType.GetFields().Where(fieldInfo => SavedStringValues.ContainsKey(fieldInfo.Name) && fieldInfo.DeclaringType == thisType)) {
                try {
                    var value = SavedStringValues[fieldInfo.Name];
                    if (fieldInfo.FieldType == typeof(Color)) {
                        fieldInfo.SetValue(this, ColorTranslator.FromHtml(GetHtmlColor(value)));
                    } else if (fieldInfo.FieldType == typeof(string)) {
                        fieldInfo.SetValue(this, value);
                    }
                } catch (Exception e) {
                    throw new Exception("Couldn't convert the color : <" + SavedStringValues[fieldInfo.Name] + "> for the field <" + fieldInfo.Name + "> for the theme <" + ThemeName + "> : " + e);
                }
            }
        }

        /// <summary>
        /// Find the html color behind any property
        /// </summary>
        public string GetHtmlColor(string propertyName) {
            return propertyName.ReplaceAliases(SavedStringValues).ApplyColorFunctions();
        }

        #endregion
    }
}