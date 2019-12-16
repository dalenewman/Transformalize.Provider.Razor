#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms.Razor {

   public class RazorTransform : BaseTransform {

      private IRazorEngineService _service;
      private Field[] _input;
      private static readonly ConcurrentDictionary<int, IRazorEngineService> Cache = new ConcurrentDictionary<int, IRazorEngineService>();
      private readonly Func<string, object> _convert;

      public RazorTransform(IContext context = null) : base(context, null) {

         if (IsMissingContext()) {
            return;
         }

         Returns = Context.Field.Type;

         IsMissing(Context.Operation.Template);

         if (Returns == "string") {
            _convert = o => (o.Trim('\n', '\r'));
         } else {
            _convert = o => Context.Field.Convert(o.Trim(' ', '\n', '\r'));
         }

      }

      public override IRow Operate(IRow row) {
         throw new NotImplementedException();
      }

      public override IEnumerable<IRow> Operate(IEnumerable<IRow> rows) {
         var fileBasedTemplate = Context.Process.Templates.FirstOrDefault(t => t.Name == Context.Operation.Template);

         if (fileBasedTemplate != null) {
            Context.Operation.Template = fileBasedTemplate.Content;
         }

         var input = MultipleInput();
         var matches = Context.Entity.GetFieldMatches(Context.Operation.Template);
         _input = input.Union(matches).ToArray();

         if (!Run)
            yield break;

         var key = GetHashCode(Context.Operation.Template, _input);
         if (!Cache.TryGetValue(key, out _service)) {
            var config = new TemplateServiceConfiguration {
               DisableTempFileLocking = true,
               EncodedStringFactory = Context.Field.Raw ? (IEncodedStringFactory)new HtmlEncodedStringFactory() : new RawStringFactory(),
               Language = Language.CSharp,
               CachingProvider = new DefaultCachingProvider(t => { })
            };
            _service = RazorEngineService.Create(config);
            Cache.TryAdd(key, _service);
         }

         var first = true;
         foreach (var row in rows) {
            if (first) {
               try {
                  var output = _service.RunCompile((string)Context.Operation.Template, (string)Context.Key, typeof(ExpandoObject), row.ToFriendlyExpandoObject(_input), null);
                  row[Context.Field] = _convert(output);
                  first = false;
               } catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex) {
                  Context.Error(ex.Message.Replace("{", "{{").Replace("}", "}}"));
                  yield break;
               } catch (TemplateCompilationException ex) {
                  Context.Error(ex.Message.Replace("{", "{{").Replace("}", "}}"));
                  yield break;
               }
               yield return row;
            } else {
               var output = _service.Run(Context.Key, typeof(ExpandoObject), row.ToFriendlyExpandoObject(_input));
               row[Context.Field] = _convert(output);
               yield return row;
            }

         }

      }

      /// <summary>
      /// Jon Skeet's Answer from Stack Overflow
      /// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
      /// http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
      /// </summary>
      /// <param name="template"></param>
      /// <param name="fields"></param>
      /// <returns></returns>
      private static int GetHashCode(string template, IEnumerable<Field> fields) {
         unchecked {
            var hash = (int)2166136261;
            hash = hash * 16777619 ^ template.GetHashCode();
            foreach (var field in fields) {
               hash = hash * 16777619 ^ field.Type.GetHashCode();
            }
            return hash;
         }
      }

      public override IEnumerable<OperationSignature> GetSignatures() {
         yield return new OperationSignature("razor") {
            Parameters = new List<OperationParameter> {
                    new OperationParameter("template")
                }
         };
      }
   }
}