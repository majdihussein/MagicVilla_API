using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private string secretkey;
        public UserRepository(ApplicationDbContext db, IConfiguration configuration
            , UserManager<ApplicationUser> userManager,IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            secretkey = configuration.GetValue<string>("APISettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return true; // اليوزر جديد 
            }
            return false; 
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower()
            == loginRequestDTO.UserName.ToLower());

            bool IsValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


            if ( user == null || IsValid == false )
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }
            // if user found we need a JWT token
            var roles = await _userManager.GetRolesAsync(user); // الحصول على صلاحيات المستخدم


            var tokenHandler = new JwtSecurityTokenHandler(); // انشأء المحرك الرئيسي لعملية التوكين
            var key = Encoding.ASCII.GetBytes(secretkey); // تحويل الكي الى نظام البايت

            var tokenDescriptor = new SecurityTokenDescriptor // وصف التوكين
            {
                Subject = new ClaimsIdentity(new Claim[] // claim هي معلومة صغيرة عن المستخدم اسمه رقمه اي شي
                {
                    new Claim(ClaimTypes.Name,user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()) // صلاحيات المستخدم
                }),
                Expires = DateTime.UtcNow.AddDays(7), // مدة صلاحية التوكين 7 ايام
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // تشفير التوكين
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); // انشاء التوكين
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                Token = tokenHandler.WriteToken(token), // تحويل التوكين الى نص
                User = _mapper.Map<UserDTO>(user) // تحويل المستخدم الى DTO
            };
            return loginResponseDTO; // ارجاع التوكين مع معلومات المستخدم
        }

        public async Task<UserDTO> Register(RegistraionRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.UserName,
                Name = registrationRequestDTO.Name,
                Email = registrationRequestDTO.UserName,
                NormalizedEmail = registrationRequestDTO.UserName.ToUpper() 
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if (result.Succeeded) 
                {
                    if(!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    
                    await _userManager.AddToRoleAsync(user, "admin");
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName.ToLower() == registrationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception ex)
            {
                
            }
            return new UserDTO();

        }
    }
}
