﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace WebForms.Helper.Page;

/// <summary>
/// Parses the view state, constructing a viaully-accessible object graph.
/// </summary>
public class ViewStateParser
{
    // private member variables
    private TextWriter tw;
    private string indentString = "   ";

    #region Constructor
    /// <summary>
    /// Creates a new ViewStateParser instance, specifying the TextWriter to emit the output to.
    /// </summary>
    public ViewStateParser(TextWriter writer)
    {
        tw = writer;
    }
    #endregion

    #region Methods
    #region ParseViewStateGraph Methods
    /// <summary>
    /// Emits a readable version of the view state to the TextWriter passed into the object's constructor.
    /// </summary>
    /// <param name="viewState">The view state object to start parsing at.</param>
    public virtual void ParseViewStateGraph(object viewState)
    {
        ParseViewStateGraph(viewState, 0, string.Empty);
    }

    /// <summary>
    /// Emits a readable version of the view state to the TextWriter passed into the object's constructor.
    /// </summary>
    /// <param name="viewStateAsString">A base-64 encoded representation of the view state to parse.</param>
    public virtual void ParseViewStateGraph(string viewStateAsString)
    {
        // First, deserialize the string into a Triplet
        LosFormatter los = new LosFormatter();
        object viewState = los.Deserialize(viewStateAsString);

        ParseViewStateGraph(viewState, 0, string.Empty);
    }

    /// <summary>
    /// Recursively parses the view state.
    /// </summary>
    /// <param name="node">The current view state node.</param>
    /// <param name="depth">The "depth" of the view state tree.</param>
    /// <param name="label">A label to display in the emitted output next to the current node.</param>
    protected virtual void ParseViewStateGraph(object node, int depth, string label)
    {
        tw.Write(System.Environment.NewLine);

        if (node == null)
        {
            tw.Write(String.Concat(Indent(depth), label, "NODE IS NULL"));
        }
        else if (node is Triplet)
        {
            tw.Write(String.Concat(Indent(depth), label, "TRIPLET"));
            ParseViewStateGraph(((Triplet)node).First, depth + 1, "First: ");
            ParseViewStateGraph(((Triplet)node).Second, depth + 1, "Second: ");
            ParseViewStateGraph(((Triplet)node).Third, depth + 1, "Third: ");
        }
        else if (node is Pair)
        {
            tw.Write(String.Concat(Indent(depth), label, "PAIR"));
            ParseViewStateGraph(((Pair)node).First, depth + 1, "First: ");
            ParseViewStateGraph(((Pair)node).Second, depth + 1, "Second: ");
        }
        else if (node is ArrayList)
        {
            tw.Write(String.Concat(Indent(depth), label, "ARRAYLIST"));

            // display array values
            for (int i = 0; i < ((ArrayList)node).Count; i++)
                ParseViewStateGraph(((ArrayList)node)[i], depth + 1, String.Format("({0}) ", i));
        }
        else if (node.GetType().IsArray)
        {
            tw.Write(String.Concat(Indent(depth), label, "ARRAY "));
            tw.Write(String.Concat("(", node.GetType().ToString(), ")"));
            IEnumerator e = ((Array)node).GetEnumerator();
            int count = 0;
            while (e.MoveNext())
                ParseViewStateGraph(e.Current, depth + 1, String.Format("({0}) ", count++));
        }
        else if (node.GetType().IsPrimitive || node is string)
        {
            tw.Write(String.Concat(Indent(depth), label));
            tw.Write(node.ToString() + " (" + node.GetType().ToString() + ")");
        }
        else if (node.GetType().ToString().Contains("IndexedString"))
        {
            tw.Write(String.Concat(Indent(depth), label));
            var indexed = (System.Web.UI.IndexedString) node;
            tw.Write(indexed.Value + " (" + node.GetType().ToString() + ")");
        }
        else
        {
            tw.Write(String.Concat(Indent(depth), label, "OTHER - "));
            tw.Write(node.GetType().ToString());
        }
    }
    #endregion

    /// <summary>
    /// Returns a string containing the <see cref="IndentString"/> property value a specified number of times.
    /// </summary>
    /// <param name="depth">The number of times to repeat the <see cref="IndentString"/> property.</param>
    /// <returns>A string containing the <see cref="IndentString"/> property value a specified number of times.</returns>
    protected virtual string Indent(int depth)
    {
        StringBuilder sb = new StringBuilder(IndentString.Length * depth);
        for (int i = 0; i < depth; i++)
            sb.Append(IndentString);

        return sb.ToString();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Specifies the indentation to use for each level when displaying the object graph.
    /// </summary>
    /// <value>A string value; the default is three blank spaces.</value>
    public string IndentString
    {
        get
        {
            return indentString;
        }
        set
        {
            indentString = value;
        }
    }
    #endregion
}
