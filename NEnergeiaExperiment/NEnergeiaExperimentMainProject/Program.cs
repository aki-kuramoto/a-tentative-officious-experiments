using System.Collections.Generic;
using System.Linq;
using ArgumentException = System.ArgumentException;
using ArgumentNullException = System.ArgumentNullException;
using Attribute = System.Attribute;
using AttributeTargets = System.AttributeTargets;
using AttributeUsageAttribute = System.AttributeUsageAttribute;
using Console = System.Console;
using Enum = System.Enum;
using Exception = System.Exception;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
// Note:
// "using System.Collections.Generic;" is for
//   importing generic interface System.Collections.Generic.IEnumerable<out T> to here.
// "using System.Linq;" is for
//   importing extension method System.Linq.Enumerable.Skip to here.
//   importing extension method System.Linq.Enumerable.Take to here.
//   importing extension method System.Linq.Enumerable.ToArray.
// I think, there is hidden "using ValueTuple = System.ValueTuple;" too.
//   To use the struct, install optional package 'System.ValueTuple'.
//   following part is a way to do that.
//     In Visual Studio Community 2017 IDE...
//       tool bar -> Tools -> NuGet Package Manager -> Package Manager Console
//       
//       PM> Find-Package System.ValueTuple [enter]
//       PM> Install-Package System.ValueTuple [enter]
//       
//       You can also uninstall with typing "Install-Package System.ValueTuple".
/// <summary>
/// [実験][例外機能の代替案] 実験の途中の物なので実用的とはいえない。
/// 
/// アリストテレスの哲学用語 dynamis, energeia, entelecheia に触発されて考え始めたが、
/// 結果としてはかなり離れてきていると感じる。
/// 概念A - 成功するか失敗するかは分からない可能性を表す。概念B と同一視できると面白い。
/// 概念B - 成功したか失敗したかは問わないが結果を表す。これを型として定義できるはず。概念A と同一視できると面白い。
/// 概念C - 結果のうち失敗したと言えるもの。そのまま継続はできないだろうし、レポートできる問題を保持のでそれに応じて進ませる。
/// 概念D - 結果のうち目的は果たしているもの。即ち一応は成功したと言える結果。何らかの問題をレポートする事はできるかもしれないが成功として進む事ができる。
/// 概念E - 一点の曇りもなく目的を完全達成していると言える結果。レポートすべき問題さえない。
/// </summary>

// ########################################
namespace dulledSharp.officiousExperiments.opinionatedStyle {
// namespace DulledSharp.OfficiousExperiments { // (community-oriented style)
// ########################################
	/// <summary>
	/// 投げる事になりそうな問題の型を宣言しておく為の属性。
	/// 考え方の方向性としては checked 例外の方向性に近いので嫌う人は嫌うだろう。
	/// </summary>
	// Java の Retention アノテーション で RetentionPolicy.CLASS を指定するような指定がしたいが見当たらない。
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class DeclaredExceptionAttribute : Attribute
	{
		/// <summary>
		/// 投げることになる問題の型を示す。
		/// </summary>
		private System.Type _exceptionType;
		
		/// <summary>
		/// 投げることになる問題の型を受け取るコンストラクター
		/// </summary>
		/// <param name="exceptionType"></param>
		public DeclaredExceptionAttribute(System.Type exceptionType)
		{
			// "問題" を例外に限るのか、それともそれ以外の類の問題も含めるのかは要検討だが
			// 一旦は例外に限って検討を進める。
			if (!((exceptionType.Equals(typeof(Exception))) || (exceptionType.IsSubclassOf(typeof(Exception)))))
			{
				// 指定された型が例外ではない場合。
				throw new Exception("Argument exceptionType must be a Exception.");
			}
			
			this._exceptionType = exceptionType;
		}
		
		/// <summary>
		/// 投げることになる例外の型を取得します。
		/// </summary>
		public System.Type exceptionType => this._exceptionType;
	}
	
	/// <summary>
	/// 手続きが本来返したい結果そのものと発生するかもしれない問題をくっつけた型。
	/// 本来返したい結果そのものと違う事をなるべく意識せずに済むように暗黙演算子を定義したりしてみる(暗黙なのが吉と出るか凶と出るか...)。
	/// </summary>
	/// <typeparam name="RESULT_TYPE">The type of intrinsic result.</typeparam>
	struct Exceptional<RESULT_TYPE>
	{
		/// <summary>
		/// The cast operator for converting an energeia to an intrinsic result value.
		/// This operator converts an energeia into a result value implicitly.
		/// When the energeia contains a problem, supposed exception will be thrown.
		/// </summary>
		public static implicit operator RESULT_TYPE(Exceptional<RESULT_TYPE> energeia)
		{
			if (energeia.error != null)
			{
				Console.WriteLine(energeia.error.stackTrace);
				throw (Exception)energeia.error;
			}
			
			return energeia._value;
		}
		
		/// <summary>
		/// The cast operator for converting an intrinsic result value to an energeia.
		/// This operator casts a concrete result value to an energeia as an entelecheia. (It means that there are no problems.)
		/// </summary>
		public static implicit operator Exceptional<RESULT_TYPE>(RESULT_TYPE resultValue)
		{
			return new Exceptional<RESULT_TYPE>(resultValue, null);
		}
		
		/// <summary>
		/// The cast operator for converting an exception to an energeia.
		/// This operator casts an exception to an energeia as an error. (In this case, there is no significant result value.)
		/// </summary>
		public static implicit operator Exceptional<RESULT_TYPE>(Exception exception)
		{
			Error<Exception> error = exception;
			return new Exceptional<RESULT_TYPE>(default, error);
		}
		
		/// <summary>
		/// The cast operator for converting a problem to an energeia.
		/// This operator casts an exception to an energeia as an error. (In this case, there is no significant result value.)
		/// </summary>
		public static implicit operator Exceptional<RESULT_TYPE>(Error<Exception> error)
		{
			return new Exceptional<RESULT_TYPE>(default, error);
		}
		
		/// <summary>
		/// The cast operator for converting a pair of insrinsic result value and exception into an energeia.
		/// This operator casts the tuple of result value and exception to an energeia.
		/// The returned energeia will be not only a result value also some problem as warning simultaneously.
		/// </summary>
		public static implicit operator Exceptional<RESULT_TYPE>((RESULT_TYPE resultValue, Exception exception) value)
		{
			return new Exceptional<RESULT_TYPE>(value.resultValue, value.exception);
		}
		
		/// <summary>
		/// 本質的な結果
		/// </summary>
		private RESULT_TYPE _value;
		
		/// <summary>
		/// 孕んでいる問題
		/// </summary>
		// TODO: Consider multiple problem reporting.
		private Error<Exception> _error;
		
		/// <summary>
		/// 本質的な結果と問題を個別に受け取るコンストラクター。
		/// これを new 演算子で直接作ってる所を読んでも、
		/// 何がしたいんだかよくわからなくなる気がする。
		/// 明示的で static な作成用のメソッドで意識的にするのか、
		/// あるいは真逆で意識させないようにキャスト演算子で作るのがいいと思う。
		/// </summary>
		/// <param name="resultValue">intrinsic result value.</param>
		/// <param name="error">problem</param>
		private Exceptional(RESULT_TYPE resultValue, Error<Exception> error)
		// noexcept(true)
		{
			this._value = resultValue;
			this._error = error;
		}
		
		/// <summary>
		/// 問題だけを受け取るコンストラクター。
		/// </summary>
		/// <param name="error"></param>
		private Exceptional(Error<Exception> error)
		{
			this._value = default;
			this._error = error;
		}
		
		/// <summary>
		/// 問題があるか?
		/// </summary>
		public bool hasError => (this._error != null);
		
		/// <summary>
		/// 問題がないか?
		/// </summary>
		public bool isNotError => (!this.hasError);
		
		/// <summary>
		/// これは entelecheia か?
		/// </summary>
		// public bool isEntelecheia => isNotError; 例え少々問題があっても本来の戻り値さえ返せるなら進む事もできる気がする。
		
		/// <summary>
		/// 孕んでいる問題
		/// </summary>
		public Error<Exception> error => this._error;
		
		/// <summary>
		/// 文字列として表現した場合
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this.hasError)
			{
				return this._error.ToString();
			}
			
			return ((RESULT_TYPE)this).ToString();
		}
	}
	
	// 例外じゃない物もある種の問題として扱えないか試してみたい。例えば enum.
	class ProblemBasedOnEnum<ENUM> where ENUM : Enum
	{
		// TODO
	}
	
	/// <summary>
	/// 例外をある種の問題の一つとする時のクラス
	/// </summary>
	/// <typeparam name="EXCEPTION">例外</typeparam>
	class Error<EXCEPTION> where EXCEPTION : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="error"></param>
		public static explicit operator Exception(Error<EXCEPTION> error)
		{
			return error._exception;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="exception"></param>
		public static implicit operator Error<EXCEPTION>(EXCEPTION exception)
		{
			var stackTrace = new AlternativeStackTrace(2);
			return new Error<EXCEPTION>(exception, stackTrace);
		}
		
		/// <summary>
		/// 孕んでいる例外
		/// </summary>
		private EXCEPTION _exception;
		
		/// <summary>
		/// スタックトレース
		/// </summary>
		private AlternativeStackTrace _stackTrace;
		
		/// <summary>
		/// 例外とスタックトレースから作るコンストラクター。
		/// スタックトレースの指定が非常に難しいのでそのままは使えないだろう。
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="stackTrace"></param>
		private Error(EXCEPTION exception, AlternativeStackTrace stackTrace)
		{
			this._exception = exception;
			this._stackTrace = stackTrace;
		}
		
		// スタックトレース
		public AlternativeStackTrace stackTrace => this._stackTrace;
	}
	
	/// <summary>
	/// これは System.Diagnostics.StackTrace が上手く作れないから代わりに仮に作っただけ。
	/// スタックフレームの何段目からなのかを指定するのが難しい。
	/// </summary>
	class AlternativeStackTrace
	{
		public AlternativeStackTrace()
			: this(1)
		{
		}
		
		public AlternativeStackTrace(int skipCount)
		{
			this._skipCount = skipCount + 1;
			this._rawStackTrace = new System.Diagnostics.StackTrace();
		}
		
		private StackTrace _rawStackTrace;
		private int _skipCount = 0;
		
		
		public StackFrame[] frames
		{
			get
			{
				StackFrame[] rawStackFrames = this._rawStackTrace.GetFrames();
				IEnumerable<StackFrame> cuedStackFrames = rawStackFrames.Skip(this._skipCount);
				int requestCount = this._rawStackTrace.FrameCount - this._skipCount;
				IEnumerable<StackFrame> slicedStackFrames = cuedStackFrames.Take(requestCount);
				StackFrame[] result = slicedStackFrames.ToArray();
				return result;
			}
		}
	}
	
	
	// 実験用1
	class ExampleClass1
	{
		[DeclaredException(typeof(ArgumentException))]
		[DeclaredException(typeof(ArgumentNullException))]
		public Exceptional<int> DoHoge(bool returnsError)
		{
			if (returnsError)
			{
				// return new Exception("問題がありました");
				return (Error<Exception>)(new Exception("問題がありました"));
			}
			
			return 0;
		}
	}
	
	// 実験用2
	class ExampleClass2
	{
		public static void DoSomething()
		{
			var exampleObject = new ExampleClass1();
			
			// System.Type typeOfXyz = exampleObject.GetType();
			// System.Reflection.MemberInfo memberInfo = typeOfXyz.GetMethod("DoHoge");
			// var pre = Attribute.GetCustomAttributes(memberInfo, typeof (DeclaredExceptionAttribute));
			// DeclaredExceptionAttribute[] hoges = (DeclaredExceptionAttribute[])pre;
			// foreach (var hoge in hoges)
			// {
			//     Console.Write(hoge.exceptionType);
			// }
			
			// var stackTrace = new AlternativeStackTrace(1);
			// Console.WriteLine(stackTrace);
			
			// Error<Exception> a = (Error<Exception>)new Exception("abc");
			// Console.WriteLine(a.stackTrace);
			
			Exceptional<int> ret1 = exampleObject.DoHoge(returnsError: false);
			Console.WriteLine($"ret1 は {ret1}.");
			
			var ret2 = exampleObject.DoHoge(returnsError: true);
			Console.WriteLine($"ret2 は {ret2}.");
			
			var ret3 = exampleObject.DoHoge(returnsError: true);
			if (ret3.hasError)
			{
				Console.WriteLine(ret3.error.stackTrace);
			}
			
			int ret4 = exampleObject.DoHoge(returnsError: false);
			Console.WriteLine($"ret4 は {ret4}.");
			
			// int ret5 = exampleObject.DoHoge(returnsError: true);
			
			// 終了待機
			Console.ReadKey();
		}
	}

// ########################################
} // corresponds namespace starter brace.
// ########################################


namespace NEnergeiaExperimentMainProject
{
	class Program
	{
		static void Main(string[] args)
		{
			dulledSharp.officiousExperiments.opinionatedStyle.ExampleClass2.DoSomething();
		}
	}
}

