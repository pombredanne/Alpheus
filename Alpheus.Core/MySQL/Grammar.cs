﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Alpheus
{
    public partial class MySQL
    {
        public Parser<MySQL> Parser { get;
        }
        public class Grammar : Grammar<MySQL, KeyValueSection, KeyValueNode>
        {
            public static Parser<AString> SectionNameAString
            {
                get
                {
                    return AStringFromIdentifierChar(AlphaNumericIdentifierChar.Or(Underscore));
                }
            }

            public static Parser<AString> KeyValueAString
            {
                get
                {
                    return Parse.AnyChar.Except(SingleQuote.Or(DoubleQuote).Or(Parse.Char('\n')).Or(Parse.Char('\r'))).Many().Text().Select(s => new AString(s)).Positioned();
                }
              
            }
     
            public static Parser<AString> SectionName
            {
                get
                {
                    return
                        from w1 in OptionalMixedWhiteSpace
                        from ob in OpenSquareBracket
                        from sn in SectionNameAString
                        from cb in CloseSquareBracket
                        select sn;
                }
            }

            public static Parser<KeyValueNode> SingleValuedKey
            {
                get
                {
                    return
                        from k in AlphaNumericAString
                        from e in Equal.Token()
                        from v in KeyValueAString
                        select new KeyValueNode(k, v);
                }
            }

            public static Parser<KeyValueNode> MultiValuedKey
            {
                get
                {
                    return
                        from k in AlphaNumericAString
                        from e in Equal.Token()
                        from v in KeyValueAString.DelimitedBy(Comma)
                            .Select(value => new AString
                            {
                                Position = value.First().Position,
                                Length = value.Sum(l => l.Length),
                                StringValue = string.Join(",", value),
                            })
                        select new KeyValueNode(k, v);
                }
            }

            public static Parser<KeyValueNode> Key
            {
               get
                {
                    return
                        from w in OptionalMixedWhiteSpace
                        from k in (MultiValuedKey).XOr(SingleValuedKey)
                        select k;
                }
            }
            public static Parser<CommentNode> Comment
            {
                get
                {
                    return
                        from w in OptionalMixedWhiteSpace
                        from c in SemiColon.Or(Hash).Token()
                        from a in AnyCharAString
                        select new CommentNode("Comment " + a.Position.Line, a);
                }
            }

            public static Parser<KeyValueSection> Section
            {
                get
                {
                    return
                        from w1 in OptionalMixedWhiteSpace
                        from sn in SectionName
                        from ck in Key.Or(Comment).Many()
                        select new KeyValueSection(sn, ck);

                }
            }

            public static Parser<IEnumerable<KeyValueSection>> ConfigurationTree
            {
                get
                {
                    return
                        from g1 in Key.Or(Comment).AtLeastOnce().Optional()
                        from s in Section.Many()
                        
                        select s;
                }
            }
        }
    }

}
