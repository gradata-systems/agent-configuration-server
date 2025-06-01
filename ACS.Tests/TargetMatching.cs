using ACS.Shared.Models;
using ACS.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using static ACS.Shared.Services.TargetMatchingService;

namespace ACS.Tests
{
    [TestClass]
    public class TargetMatching
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private ITargetMatchingService _targetMatchingService;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [TestInitialize]
        public void Init()
        {
            ServiceCollection services = new();
            services.AddScoped<ITargetMatchingService, TargetMatchingService>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _targetMatchingService = serviceProvider.GetRequiredService<ITargetMatchingService>();
        }

        [TestMethod]
        public void InvalidVersion()
        {
            CompiledTarget target = new TargetBuilder("agentName").Build();
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.x",
                HostName = "localhost",
                UserName = "test.user",
                EnvironmentName = "development"
            };

            Assert.ThrowsException<InvalidVersionException>(() => _targetMatchingService.IsMatch(target, requestParams));
        }

        [TestMethod]
        public void AnyMatch()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostName = "localhost",
                UserName = "test.user",
                EnvironmentName = "development"
            };

            CompiledTarget target = new TargetBuilder("agentName").Build();

            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "No criteria results in a match");
        }

        [TestMethod]
        public void MatchesVersionInRange()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostName = "localhost",
                UserName = "test.user",
                EnvironmentName = "development"
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .AgentMinVersion("1.0.0.0")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Agent version greater than min version should match");

            requestParams.AgentVersion = "0.1";
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Agent version less than min version should not match");

            target = new TargetBuilder("agentName")
                .AgentMaxVersion("1.2.0.0")
                .Build();
            requestParams.AgentVersion = "1.1";
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Agent version less than max version should match");

            requestParams.AgentVersion = "1.3";
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Agent version greater than max version should not match");
        }

        [TestMethod]
        public void MatchesUserName()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostName = "localhost",
                UserName = "test.user",
                EnvironmentName = "development"
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .UserNamePattern(@"test\..+")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Username matching the pattern should result in a match");

            requestParams.UserName = "another.user";
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Username not matching the pattern should not result in a match");
        }

        [TestMethod]
        public void MatchesActiveUser()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                ActiveUsers = ["test.user1", "test.user2"]
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .ActiveUserNamePattern(@"^test\.user.+$")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Active user matching the pattern should result in a match");

            requestParams.ActiveUsers = ["other.user"];
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Active user not matching the pattern should not result in a match");
        }

        [TestMethod]
        public void MatchesHostName()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostName = "myhost.example.com",
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .HostNamePattern(@"^.*\.example\.com$")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Host matching the pattern should result in a match");

            requestParams.HostName = "google.com";
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Host not matching the pattern should not result in a match");
        }

        [TestMethod]
        public void MatchesHostIpv4Address()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostIpv4Addresses = [
                    "192.168.1.10",
                    "10.0.10.5"
                ]
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .HostIpv4Cidr("10.0.10.0/24")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Host with an IP in range should result in a match");

            requestParams.HostIpv4Addresses = ["192.168.1.1"];
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Host with an IP out of range should not result in a match");
        }

        [TestMethod]
        public void MatchesHostRole()
        {
            ConfigQueryRequestParams requestParams = new()
            {
                AgentName = "agentName",
                AgentVersion = "1.0.1",
                HostRoles = ["ADCS", "ADDS"]
            };

            CompiledTarget target = new TargetBuilder("agentName")
                .HostRolePattern(@"^AD.+$")
                .Build();
            Assert.IsTrue(_targetMatchingService.IsMatch(target, requestParams), "Host role matching the pattern should result in a match");

            requestParams.HostRoles = ["role"];
            Assert.IsFalse(_targetMatchingService.IsMatch(target, requestParams), "Host role not matching the pattern should not result in a match");
        }

        private class TargetBuilder
        {
            private readonly Target _target;

            public TargetBuilder(string agentName) {
                _target = new Target
                {
                    AgentName = agentName,
                    Enabled = true,
                    Created = DateTime.Now,
                    CreatedBy = "test.user",
                    Modified = DateTime.Now,
                    ModifiedBy = "test.user"
                };
            }

            public CompiledTarget Build()
            {
                return new CompiledTarget(_target);
            }

            public TargetBuilder AgentMinVersion(string version)
            {
                _target.AgentMinVersion = version;
                return this;
            }

            public TargetBuilder AgentMaxVersion(string version)
            {
                _target.AgentMaxVersion = version;
                return this;
            }

            public TargetBuilder UserNamePattern(string pattern)
            {
                _target.UserNamePattern = pattern;
                return this;
            }

            public TargetBuilder ActiveUserNamePattern(string pattern)
            {
                _target.ActiveUserNamePattern = pattern;
                return this;
            }

            public TargetBuilder HostNamePattern(string pattern)
            {
                _target.HostNamePattern = pattern;
                return this;
            }

            public TargetBuilder HostIpv4Cidr(string ipv4Addresses)
            {
                _target.HostIpv4Cidr = ipv4Addresses;
                return this;
            }

            public TargetBuilder HostRolePattern(string pattern)
            {
                _target.HostRolePattern = pattern;
                return this;
            }

            public TargetBuilder Enabled(bool enabled)
            {
                _target.Enabled = enabled;
                return this;
            }
        }
    }
}