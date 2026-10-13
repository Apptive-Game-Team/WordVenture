using System;
using System.Linq;
using NUnit.Framework;

namespace WordVenture.Tests
{
    public sealed class AccountFailureTests
    {
        static Type FailureType => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Account.AccountFailure")).First(type => type != null);

        static object Context(string name) => Enum.Parse(
            AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("Account.FailureContext")).First(type => type != null), name);

        static string Describe(string context, long status, string body) =>
            (string)FailureType.GetMethod("Describe").Invoke(null, new[] { Context(context), status, body });

        static string Message(string constant) => (string)FailureType.GetField(constant).GetRawConstantValue();

        static bool Check(string method, long status, string body) =>
            (bool)FailureType.GetMethod(method).Invoke(null, new object[] { status, body });

        [TestCase("Credentials")]
        [TestCase("SignUp")]
        [TestCase("Session")]
        public void 상태_코드_0은_어느_상황에서나_네트워크_실패_문구이다(string context)
        {
            Assert.That(Describe(context, 0, ""), Is.EqualTo(Message("Network")));
        }

        [TestCase(401)]
        [TestCase(400)]
        public void 로그인_실패는_잘못된_이메일_또는_비밀번호_문구이다(int status)
        {
            Assert.That(Describe("Credentials", status, "Unauthorized"), Is.EqualTo(Message("WrongCredentials")));
        }

        [Test]
        public void 가입_실패는_서버_메시지가_있으면_그대로_보여_준다()
        {
            Assert.That(Describe("SignUp", 400, "비밀번호는 8자 이상이어야 합니다."), Is.EqualTo("비밀번호는 8자 이상이어야 합니다."));
            Assert.That(Describe("SignUp", 400, ""), Is.EqualTo(Message("Unknown")));
            Assert.That(Describe("SignUp", 400, new string('가', 101)), Is.EqualTo(Message("Unknown")));
        }

        [TestCase(401)]
        [TestCase(302)]
        public void 인증이_필요한_요청의_401과_302는_다시_로그인하라는_문구이다(int status)
        {
            Assert.That(Describe("Session", status, ""), Is.EqualTo(Message("LoginAgain")));
        }

        [Test]
        public void 서버_오류와_시스템_미등록을_구분한다()
        {
            Assert.That(Describe("Session", 500, ""), Is.EqualTo(Message("ServerError")));
            Assert.That(Describe("Session", 400, "System not found: wordventure"), Is.EqualTo(Message("SystemMissing")));
            Assert.That(Describe("Session", 400, "Key-Value not found memberId: 1"), Is.EqualTo(Message("Unknown")));
        }

        [Test]
        public void 없는_key_와_없는_시스템은_400_본문으로_구분한다()
        {
            Assert.That(Check("IsKeyMissing", 400, "Key-Value not found memberId: 1, systemId: 2, key: save"), Is.True);
            Assert.That(Check("IsKeyMissing", 400, "System not found: wordventure"), Is.False);
            Assert.That(Check("IsKeyMissing", 500, "Key-Value not found"), Is.False);
            Assert.That(Check("IsKeyMissing", 400, null), Is.False);
            Assert.That(Check("IsSystemMissing", 400, "System not found: wordventure"), Is.True);
            Assert.That(Check("IsSystemMissing", 400, "Key-Value not found"), Is.False);
        }
    }
}
