using System;
// ReSharper disable CheckNamespace GrammarMistakeInComment CommentTypo InconsistentNaming

namespace JetBrains.Annotations
{
    /// <summary>
    /// Indicates that the marked parameter is a message template where placeholders are to be replaced by
    /// the following arguments in the order in which they appear.
    /// </summary>
    /// <example><code>
    /// void LogInfo([StructuredMessageTemplate]string message, params object[] args) { /* do something */ }
    /// 
    /// void Foo() {
    ///   LogInfo("User created: {username}"); // Warning: Non-existing argument in format string
    /// }
    /// </code></example>
    /// <seealso cref="StringFormatMethodAttribute"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class StructuredMessageTemplateAttribute : Attribute;

    /// <summary>
    /// Indicates that the integral value falls into the specified interval.
    /// It is allowed to specify multiple non-intersecting intervals.
    /// Values of interval boundaries are included in the interval.
    /// </summary>
    /// <example><code>
    /// void Foo([ValueRange(0, 100)] int value) {
    ///   if (value == -1) { // Warning: Expression is always 'false'
    ///     ...
    ///   }
    /// }
    /// </code></example>
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Method | AttributeTargets.Delegate,
        AllowMultiple = true)]
    public sealed class ValueRangeAttribute : Attribute
    {
        public object From { get; }
        public object To { get; }

        public ValueRangeAttribute(long from, long to) : this((object)from, to) { }
        public ValueRangeAttribute(ulong from, ulong to) : this((object)from, to) { }
        public ValueRangeAttribute(long value) : this((object)value, value) { }
        public ValueRangeAttribute(ulong value) : this((object)value, value) { }
        private ValueRangeAttribute(object from, object to)
        {
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Indicates that the integral value never falls below zero.
    /// </summary>
    /// <example><code>
    /// void Foo([NonNegativeValue] int value) {
    ///   if (value == -1) { // Warning: Expression is always 'false'
    ///     ...
    ///   }
    /// }
    /// </code></example>
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Method | AttributeTargets.Delegate)]
    public sealed class NonNegativeValueAttribute : Attribute;

    /// <summary>
    /// Indicates that the method or type uses equality members of the annotated element.
    /// </summary>
    /// <remarks>
    /// When applied to the method's generic parameter, indicates that the equality of the annotated type is used,
    /// unless a custom equality comparer is passed when calling this method. The attribute can also be applied
    /// directly to the method's parameter or return type to specify equality usage for it.
    /// When applied to the type's generic parameter, indicates that type equality usage can happen anywhere
    /// inside this type, so the instantiation of this type is treated as equality usage, unless a custom
    /// equality comparer is passed to the constructor.
    /// </remarks>
    /// <example><code>
    /// struct StructWithDefaultEquality { }
    /// 
    /// class MySet&lt;[DefaultEqualityUsage] T&gt; { }
    /// 
    /// static class Extensions {
    ///     public static MySet&lt;T&gt; ToMySet&lt;[DefaultEqualityUsage] T&gt;(this IEnumerable&lt;T&gt; items) =&gt; new();
    /// }
    /// 
    /// class MyList&lt;T&gt; { public int IndexOf([DefaultEqualityUsage] T item) =&gt; 0; }
    /// 
    /// class UsesDefaultEquality {
    ///     void Test() {
    ///         var list = new MyList&lt;StructWithDefaultEquality&gt;();
    ///         list.IndexOf(new StructWithDefaultEquality()); // Warning: Default equality of struct 'StructWithDefaultEquality' is used
    ///         
    ///         var set = new MySet&lt;StructWithDefaultEquality&gt;(); // Warning: Default equality of struct 'StructWithDefaultEquality' is used
    ///         var set2 = new StructWithDefaultEquality[1].ToMySet(); // Warning: Default equality of struct 'StructWithDefaultEquality' is used
    ///     }
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class DefaultEqualityUsageAttribute : Attribute;

    /// <summary>
    /// Indicates that method or class instance acquires resource ownership and will dispose it after use.
    /// </summary>
    /// <remarks>
    /// Annotation of <c>out</c> parameters with this attribute is meaningless.<br/>
    /// When an instance method is annotated with this attribute,
    /// it means that it is handling the resource disposal of the corresponding resource instance.<br/>
    /// When a field or a property is annotated with this attribute, it means that this type owns the resource
    /// and will handle the resource disposal properly (e.g. in own <c>IDisposable</c> implementation).
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class HandlesResourceDisposalAttribute : Attribute;

    /// <summary>
    /// This annotation allows enforcing allocation-less usage patterns of delegates for performance-critical APIs.
    /// When this annotation is applied to the parameter of a delegate type,
    /// the IDE checks the input argument of this parameter:
    /// * When a lambda expression or anonymous method is passed as an argument, the IDE verifies that the passed closure
    ///   has no captures of the containing local variables and the compiler is able to cache the delegate instance
    ///   to avoid heap allocations. Otherwise, a warning is produced.
    /// * The IDE warns when the method name or local function name is passed as an argument because this always results
    ///   in heap allocation of the delegate instance.
    /// </summary>
    /// <remarks>
    /// In C# 9.0+ code, the IDE will also suggest annotating the anonymous functions with the <c>static</c> modifier
    /// to make use of the similar analysis provided by the language/compiler.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RequireStaticDelegateAttribute : Attribute
    {
        public bool IsError { get; set; }
    }

    /// <summary>
    /// Language of the injected code fragment inside a string literal marked by the <see cref="LanguageInjectionAttribute"/>.
    /// </summary>
    public enum InjectedLanguage
    {
        CSS = 0,
        HTML = 1,
        JAVASCRIPT = 2,
        JSON = 3,
        XML = 4,
    }

    /// <summary>
    /// Indicates that the marked parameter, field, or property accepts string literals
    /// containing code fragments in a specified language.
    /// </summary>
    /// <example><code>
    /// void Foo([LanguageInjection(InjectedLanguage.CSS, Prefix = "body{", Suffix = "}")] string cssProps)
    /// {
    ///   // cssProps should only contain a list of CSS properties
    /// }
    /// </code></example>
    /// <example><code>
    /// void Bar([LanguageInjection("json")] string json)
    /// {
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class LanguageInjectionAttribute : Attribute
    {
        /// <summary>Specifies a language of the injected code fragment.</summary>
        public InjectedLanguage InjectedLanguage { get; }
        /// <summary>Specifies a language name of the injected code fragment.</summary>
        public string? InjectedLanguageName { get; }
        /// <summary>Specifies a string that 'precedes' the injected string literal.</summary>
        public string? Prefix { get; set; }
        /// <summary>Specifies a string that 'follows' the injected string literal.</summary>
        public string? Suffix { get; set; }

        public LanguageInjectionAttribute(InjectedLanguage injectedLanguage)
            => InjectedLanguage = injectedLanguage;
        public LanguageInjectionAttribute(string injectedLanguage)
            => InjectedLanguageName = injectedLanguage;
    }

    /// <summary>
    /// Defines a code search pattern using the Structural Search and Replace syntax.
    /// It allows you to find and, if necessary, replace blocks of code that match a specific pattern.
    /// </summary>
    /// <remarks>
    /// Search and replace patterns consist of a textual part and placeholders.
    /// Textural part must contain only identifiers allowed in the target language and will be matched exactly
    /// (whitespaces, tabulation characters, and line breaks are ignored).
    /// Placeholders allow matching variable parts of the target code blocks.
    /// <br/>
    /// A placeholder has the following format:
    /// <c>$placeholder_name$</c> - where <c>placeholder_name</c> is an arbitrary identifier.
    /// Predefined placeholders:
    /// <list type="bullet">
    /// <item><c>$this$</c> - expression of containing type</item>
    /// <item><c>$thisType$</c> - containing type</item>
    /// <item><c>$member$</c> - current member placeholder</item>
    /// <item><c>$qualifier$</c> - this placeholder is available in the replace pattern and can be used
    /// to insert a qualifier expression matched by the <c>$member$</c> placeholder.
    /// (Note that if <c>$qualifier$</c> placeholder is used,
    /// then <c>$member$</c> placeholder will match only qualified references)</item>
    /// <item><c>$expression$</c> - expression of any type</item>
    /// <item><c>$identifier$</c> - identifier placeholder</item>
    /// <item><c>$args$</c> - any number of arguments</item>
    /// <item><c>$arg$</c> - single argument</item>
    /// <item><c>$arg1$ ... $arg10$</c> - single argument</item>
    /// <item><c>$stmts$</c> - any number of statements</item>
    /// <item><c>$stmt$</c> - single statement</item>
    /// <item><c>$stmt1$ ... $stmt10$</c> - single statement</item>
    /// <item><c>$name{Expression, 'Namespace.FooType'}$</c> - expression with the <c>Namespace.FooType</c> type</item>
    /// <item><c>$expression{'Namespace.FooType'}$</c> - expression with the <c>Namespace.FooType</c> type</item>
    /// <item><c>$name{Type, 'Namespace.FooType'}$</c> - <c>Namespace.FooType</c> type</item>
    /// <item><c>$type{'Namespace.FooType'}$</c> - <c>Namespace.FooType</c> type</item>
    /// <item><c>$statement{1,2}$</c> - 1 or 2 statements</item>
    /// </list>
    /// You can also define your own placeholders of the supported types and specify arguments for each placeholder type.
    /// This can be done using the following format: <c>$name{type, arguments}$</c>. Where
    /// <c>name</c> - is the name of your placeholder,
    /// <c>type</c> - is the type of your placeholder
    /// (one of the following: Expression, Type, Identifier, Statement, Argument, Member),
    /// <c>arguments</c> - a list of arguments for your placeholder. Each placeholder type supports its own arguments.
    /// Check the examples below for more details.
    /// The placeholder type may be omitted and determined from the placeholder name,
    /// if the name has one of the following prefixes:
    /// <list type="bullet">
    /// <item>expr, expression - expression placeholder, e.g. <c>$exprPlaceholder{}$</c>, <c>$expressionFoo{}$</c></item>
    /// <item>arg, argument - argument placeholder, e.g. <c>$argPlaceholder{}$</c>, <c>$argumentFoo{}$</c></item>
    /// <item>ident, identifier - identifier placeholder, e.g. <c>$identPlaceholder{}$</c>, <c>$identifierFoo{}$</c></item>
    /// <item>stmt, statement - statement placeholder, e.g. <c>$stmtPlaceholder{}$</c>, <c>$statementFoo{}$</c></item>
    /// <item>type - type placeholder, e.g. <c>$typePlaceholder{}$</c>, <c>$typeFoo{}$</c></item>
    /// <item>member - member placeholder, e.g. <c>$memberPlaceholder{}$</c>, <c>$memberFoo{}$</c></item>
    /// </list>
    /// </remarks>
    /// <para>
    /// Expression placeholder arguments:
    /// <list type="bullet">
    /// <item>expressionType - string value in single quotes, specifies full type name to match
    /// (empty string by default)</item>
    /// <item>exactType - boolean value, specifies if expression should have exact type match (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myExpr{Expression, 'Namespace.FooType', true}$</c> - defines an expression placeholder
    /// matching expressions of the <c>Namespace.FooType</c> type with exact matching.</item>
    /// <item><c>$myExpr{Expression, 'Namespace.FooType'}$</c> - defines an expression placeholder
    /// matching expressions of the <c>Namespace.FooType</c> type or expressions that can be
    /// implicitly converted to <c>Namespace.FooType</c>.</item>
    /// <item><c>$myExpr{Expression}$</c> - defines an expression placeholder matching expressions of any type.</item>
    /// <item><c>$exprFoo{'Namespace.FooType', true}$</c> - defines an expression placeholder
    /// matching expressions of the <c>Namespace.FooType</c> type with exact matching.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Type placeholder arguments:
    /// <list type="bullet">
    /// <item>type - string value in single quotes, specifies the full type name to match (empty string by default)</item>
    /// <item>exactType - boolean value, specifies whether the expression should have the exact type match
    /// (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myType{Type, 'Namespace.FooType', true}$</c> - defines a type placeholder
    /// matching <c>Namespace.FooType</c> types with exact matching.</item>
    /// <item><c>$myType{Type, 'Namespace.FooType'}$</c> - defines a type placeholder matching <c>Namespace.FooType</c>
    /// types or types that can be implicitly converted to <c>Namespace.FooType</c>.</item>
    /// <item><c>$myType{Type}$</c> - defines a type placeholder matching any type.</item>
    /// <item><c>$typeFoo{'Namespace.FooType', true}$</c> - defines a type placeholder matching <c>Namespace.FooType</c>
    /// types with exact matching.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Identifier placeholder arguments:
    /// <list type="bullet">
    /// <item>nameRegex - string value in single quotes, specifies regex to use for matching (empty string by default)</item>
    /// <item>nameRegexCaseSensitive - boolean value, specifies if name regex is case-sensitive (true by default)</item>
    /// <item>type - string value in single quotes, specifies full type name to match (empty string by default)</item>
    /// <item>exactType - boolean value, specifies if expression should have exact type match (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myIdentifier{Identifier, 'my.*', false, 'Namespace.FooType', true}$</c> -
    /// defines an identifier placeholder matching identifiers (ignoring case) starting with <c>my</c> prefix with
    /// <c>Namespace.FooType</c> type.</item>
    /// <item><c>$myIdentifier{Identifier, 'my.*', true, 'Namespace.FooType', true}$</c> -
    /// defines an identifier placeholder matching identifiers (case sensitively) starting with <c>my</c> prefix with
    /// <c>Namespace.FooType</c> type.</item>
    /// <item><c>$identFoo{'my.*'}$</c> - defines an identifier placeholder matching identifiers (case sensitively)
    /// starting with <c>my</c> prefix.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Statement placeholder arguments:
    /// <list type="bullet">
    /// <item>minimalOccurrences - minimal number of statements to match (-1 by default)</item>
    /// <item>maximalOccurrences - maximal number of statements to match (-1 by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myStmt{Statement, 1, 2}$</c> - defines a statement placeholder matching 1 or 2 statements.</item>
    /// <item><c>$myStmt{Statement}$</c> - defines a statement placeholder matching any number of statements.</item>
    /// <item><c>$stmtFoo{1, 2}$</c> - defines a statement placeholder matching 1 or 2 statements.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Argument placeholder arguments:
    /// <list type="bullet">
    /// <item>minimalOccurrences - minimal number of arguments to match (-1 by default)</item>
    /// <item>maximalOccurrences - maximal number of arguments to match (-1 by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myArg{Argument, 1, 2}$</c> - defines an argument placeholder matching 1 or 2 arguments.</item>
    /// <item><c>$myArg{Argument}$</c> - defines an argument placeholder matching any number of arguments.</item>
    /// <item><c>$argFoo{1, 2}$</c> - defines an argument placeholder matching 1 or 2 arguments.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Member placeholder arguments:
    /// <list type="bullet">
    /// <item>docId - string value in single quotes, specifies XML documentation ID of the member to match (empty by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>$myMember{Member, 'M:System.String.IsNullOrEmpty(System.String)'}$</c> -
    /// defines a member placeholder matching <c>IsNullOrEmpty</c> member of the <c>System.String</c> type.</item>
    /// <item><c>$memberFoo{'M:System.String.IsNullOrEmpty(System.String)'}$</c> -
    /// defines a member placeholder matching <c>IsNullOrEmpty</c> member of the <c>System.String</c> type.</item>
    /// </list>
    /// </para>
    /// <seealso href="https://www.jetbrains.com/help/resharper/Navigation_and_Search__Structural_Search_and_Replace.html">
    /// Structural Search and Replace</seealso>
    /// <seealso href="https://www.jetbrains.com/help/resharper/Code_Analysis__Find_and_Update_Obsolete_APIs.html">
    /// Find and update deprecated APIs</seealso>
    [AttributeUsage(
      AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface
    | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum,
      AllowMultiple = true,
      Inherited = false)]
    public sealed class CodeTemplateAttribute(string searchTemplate) : Attribute
    {
        /// <summary>
        /// Structural search pattern.
        /// </summary>
        /// <remarks>
        /// The pattern includes a textual part, which must only contain identifiers allowed in the target language
        /// and placeholders to match variable parts of the target code blocks.
        /// </remarks>
        public string SearchTemplate { get; } = searchTemplate;
        /// <summary>
        /// Message to show when a code block matching the search pattern was found.
        /// </summary>
        /// <remarks>
        /// You can also prepend the message text with 'Error:', 'Warning:', 'Suggestion:' or 'Hint:' prefix
        /// to specify the pattern severity.
        /// Code patterns with replace templates have the 'Suggestion' severity by default.
        /// If a replace pattern is not provided, the pattern will have the 'Warning' severity.
        ///</remarks>
        public string? Message { get; set; }
        /// <summary>
        /// Replace pattern to use for replacing a matched pattern.
        /// </summary>
        public string? ReplaceTemplate { get; set; }
        /// <summary>
        /// Replace message to show in the light bulb.
        /// </summary>
        public string? ReplaceMessage { get; set; }
        /// <summary>
        /// Apply code formatting after code replacement.
        /// </summary>
        public bool FormatAfterReplace { get; set; } = true;
        /// <summary>
        /// Whether similar code blocks should be matched.
        /// </summary>
        public bool MatchSimilarConstructs { get; set; }
        /// <summary>
        /// Automatically insert namespace import directives or remove qualifiers
        /// that become redundant after the template is applied.
        /// </summary>
        public bool ShortenReferences { get; set; }
        /// <summary>
        /// The string to use as a suppression key.
        /// By default, the following suppression key is used: <c>CodeTemplate_SomeType_SomeMember</c>,
        /// where 'SomeType' and 'SomeMember' are names of the associated containing type and member,
        /// to which this attribute is applied.
        /// </summary>
        public string? SuppressionKey { get; set; }
    }

    /// <summary>
    /// Indicates that the string literal passed as an argument to this parameter
    /// should not be checked for spelling or grammar errors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class IgnoreSpellingAndGrammarErrorsAttribute : Attribute;
}
