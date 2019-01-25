namespace Sitecore.Support.Forms.Core.Pipelines
{
  using Sitecore.Diagnostics;
  using Sitecore.StringExtensions;
  using Sitecore.WFFM.Abstractions.Actions;
  using Sitecore.WFFM.Abstractions.Data;
  using Sitecore.WFFM.Abstractions.Dependencies;
  using Sitecore.WFFM.Abstractions.Mail;
  using Sitecore.WFFM.Abstractions.Shared;
  using Sitecore.WFFM.Abstractions.Utils;
  using System;
  using System.Text;
  using System.Text.RegularExpressions;
  using Sitecore.Form.Core.Configuration;

  public class ProcessMessage
  {
    private readonly string srcReplacer;

    private readonly string shortHrefReplacer;

    private readonly string shortHrefMediaReplacer;

    private readonly string hrefReplacer;

    public IItemRepository ItemRepository
    {
      get;
      set;
    }

    public IFieldProvider FieldProvider
    {
      get;
      set;
    }

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

    public void ExpandTokens(ProcessMessageArgs args)
    {
      Assert.IsNotNull(this.ItemRepository, "ItemRepository");
      Assert.IsNotNull(this.FieldProvider, "FieldProvider");
      foreach (AdaptedControlResult adaptedControlResult in args.Fields)
      {
        IFieldItem fieldItem = this.ItemRepository.CreateFieldItem(this.ItemRepository.GetItem(adaptedControlResult.FieldID));
        string text = adaptedControlResult.Value;

        #region
        if (fieldItem.ClassName == "Sitecore.Form.Web.UI.Controls.CheckboxList")
        {
          string setting = Sitecore.Configuration.Settings.GetSetting(ConfigKey.ListFieldItemsDelimiterCharacter, string.Empty);
          if (!setting.IsNullOrEmpty())
          {
            string[] arg_A6_0 = text.Split(new string[]
            {
                            setting
            }, StringSplitOptions.None);
            StringBuilder stringBuilder = new StringBuilder();
            string[] array = arg_A6_0;
            for (int i = 0; i < array.Length; i++)
            {
              string arg = array[i];
              stringBuilder.AppendFormat("<item>{0}</item>", arg);
            }
            text = stringBuilder.ToString();
          }
        }
        #endregion

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
          string text2 = args.Mail.ToString();
          if (Regex.IsMatch(text2, "\\[<label id=\"" + fieldItem.ID + "\">[^<]+?</label>]"))
          {
            text2 = Regex.Replace(text2, "\\[<label id=\"" + fieldItem.ID + "\">[^<]+?</label>]", text);
          }
          if (Regex.IsMatch(text2, "\\[<label id=\"" + fieldItem.ID + "\" renderfield=\"Value\">[^<]+?</label>]"))
          {
            text2 = Regex.Replace(text2, "\\[<label id=\"" + fieldItem.ID + "\" renderfield=\"Value\">[^<]+?</label>]", adaptedControlResult.Value);
          }
          if (Regex.IsMatch(text2, "\\[<label id=\"" + fieldItem.ID + "\" renderfield=\"Text\">[^<]+?</label>]"))
          {
            text2 = Regex.Replace(text2, "\\[<label id=\"" + fieldItem.ID + "\" renderfield=\"Text\">[^<]+?</label>]", text);
          }
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