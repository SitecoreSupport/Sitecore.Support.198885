namespace Sitecore.Support.Forms.Core.Pipelines
{
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Links;
  using Sitecore.StringExtensions;
  using Sitecore.WFFM.Abstractions.Actions;
  using Sitecore.WFFM.Abstractions.Data;
  using Sitecore.WFFM.Abstractions.Dependencies;
  using Sitecore.WFFM.Abstractions.Mail;
  using Sitecore.WFFM.Abstractions.Shared;
  using Sitecore.WFFM.Abstractions.Utils;
  using System;
  using System.Net.Mail;
  using System.Text.RegularExpressions;
  using Sitecore.Form.Core.Configuration;
  using System.Text;

  public class ProcessMessage : Sitecore.Forms.Core.Pipelines.ProcessMessage
  {
    private readonly string srcReplacer;

    private readonly string shortHrefReplacer;

    private readonly string shortHrefMediaReplacer;

    private readonly string hrefReplacer;

    public ProcessMessage() : this(DependenciesManager.WebUtil)
    {
    }

    public ProcessMessage(IWebUtil webUtil)
    {
      Assert.IsNotNull(webUtil, "webUtil");
      this.srcReplacer = string.Join(string.Empty, new string[]
      {
        "src=\"",
        webUtil.GetServerUrl(),
        "/~"
      });
      this.shortHrefReplacer = string.Join(string.Empty, new string[]
      {
        "href=\"",
        webUtil.GetServerUrl(),
        "/"
      });
      this.shortHrefMediaReplacer = string.Join(string.Empty, new string[]
      {
        "href=\"",
        webUtil.GetServerUrl(),
        "/~/"
      });
      this.hrefReplacer = this.shortHrefReplacer + "~";
    }

    public new void ExpandTokens(ProcessMessageArgs args)
    {
      Sitecore.Diagnostics.Assert.IsNotNull(this.ItemRepository, "ItemRepository");
      Sitecore.Diagnostics.Assert.IsNotNull(this.FieldProvider, "FieldProvider");
      foreach (AdaptedControlResult adaptedControlResult in args.Fields)
      {
        IFieldItem fieldItem = this.ItemRepository.CreateFieldItem(this.ItemRepository.GetItem(adaptedControlResult.FieldID));
        string text = adaptedControlResult.Value;

        // Start Bug fix: 198885
        if (fieldItem.ClassName == "Sitecore.Form.Web.UI.Controls.CheckboxList")
        {
          string listFieldItemsDelimiterCharacter = Sitecore.Configuration.Settings.GetSetting(ConfigKey.ListFieldItemsDelimiterCharacter, string.Empty);
          if (!listFieldItemsDelimiterCharacter.IsNullOrEmpty() && listFieldItemsDelimiterCharacter.Length == 1)
          {
            string[] values = text.Split(Convert.ToChar(listFieldItemsDelimiterCharacter));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string current in values)
            {
              stringBuilder.AppendFormat("<item>{0}</item>", current);
            }
            text = stringBuilder.ToString();
          }
        }
        // End Bug fix: 198885

        text = this.FieldProvider.GetAdaptedValue(adaptedControlResult.FieldID, text);
        text = Regex.Replace(text, "src=\"/sitecore/shell/themes/standard/~", this.srcReplacer);
        text = Regex.Replace(text, "href=\"/sitecore/shell/themes/standard/~", this.hrefReplacer);
        text = Regex.Replace(text, "on\\w*=\".*?\"", string.Empty);
        if (args.MessageType == MessageType.Sms)
        {
          args.Mail.Replace("[{0}]".FormatWith(new object[]
          {
                        fieldItem.FieldDisplayName
          }), text);
          args.Mail.Replace("[{0}]".FormatWith(new object[]
          {
                        fieldItem.Name
          }), text);
        }
        else
        {
          if (!string.IsNullOrEmpty(adaptedControlResult.Parameters) && args.IsBodyHtml)
          {
            if (adaptedControlResult.Parameters.StartsWith("multipleline"))
            {
              text = text.Replace(Environment.NewLine, "<br/>");
            }
            if (adaptedControlResult.Parameters.StartsWith("secure") && adaptedControlResult.Parameters.Contains("<schidden>"))
            {
              text = Regex.Replace(text, "\\d", "*");
            }
          }
          string text2 = Regex.Replace(args.Mail.ToString(), "\\[<label id=\"" + fieldItem.ID + "\">[^<]+?</label>]", text);
          text2 = text2.Replace(fieldItem.ID.ToString(), text);
          args.Mail.Clear().Append(text2);
        }
        args.From = args.From.Replace("[" + fieldItem.ID + "]", text);
        args.From = args.From.Replace(fieldItem.ID.ToString(), text);
        args.To.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.ID.ToString(),
                    "]"
        }), text);
        args.To.Replace(string.Join(string.Empty, new string[]
        {
                    fieldItem.ID.ToString()
        }), text);
        args.CC.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.ID.ToString(),
                    "]"
        }), text);
        args.CC.Replace(string.Join(string.Empty, new string[]
        {
                    fieldItem.ID.ToString()
        }), text);
        args.Subject.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.ID.ToString(),
                    "]"
        }), text);
        args.From = args.From.Replace("[" + fieldItem.FieldDisplayName + "]", text);
        args.To.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.FieldDisplayName,
                    "]"
        }), text);
        args.CC.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.FieldDisplayName,
                    "]"
        }), text);
        args.Subject.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    fieldItem.FieldDisplayName,
                    "]"
        }), text);
        args.From = args.From.Replace("[" + adaptedControlResult.FieldName + "]", text);
        args.To.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    adaptedControlResult.FieldName,
                    "]"
        }), text);
        args.CC.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    adaptedControlResult.FieldName,
                    "]"
        }), text);
        args.Subject.Replace(string.Join(string.Empty, new string[]
        {
                    "[",
                    adaptedControlResult.FieldName,
                    "]"
        }), text);
      }
    }

  }
}
