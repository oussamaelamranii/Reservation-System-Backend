using Reservation_System_Backend.Models;

namespace Reservation_System_Backend.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return; // Already seeded

        // --- Seed Users ---
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@coach.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "admin"
        };

        var clientUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "client"
        };

        context.Users.AddRange(adminUser, clientUser);

        // --- Seed Sessions ---
        var today = DateTime.UtcNow.Date;
        // Get Monday of this week
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = today.AddDays(-diff);

        var sessions = new List<Session>
        {
            new Session
            {
                Id = Guid.NewGuid(),
                Title = "Yoga Flow Matinal",
                Description = "Commencez votre journée avec énergie et sérénité. Une séance de Vinyasa doux adaptée à tous les niveaux.",
                CoachName = "Sophie Martin",
                CoachBio = "Professeure certifiée de Yoga Vinyasa avec 5 ans d'expérience.",
                Date = monday,
                StartTime = "09:00",
                EndTime = "10:00",
                Capacity = 10
            },
            new Session
            {
                Id = Guid.NewGuid(),
                Title = "HIIT Intense",
                Description = "Brûlez un maximum de calories avec cet entraînement fractionné à haute intensité.",
                CoachName = "Marc Dubois",
                CoachBio = "Coach sportif spécialisé en renforcement musculaire et cardio.",
                Date = monday.AddDays(1),
                StartTime = "18:30",
                EndTime = "19:30",
                Capacity = 10
            },
            new Session
            {
                Id = Guid.NewGuid(),
                Title = "Pilates Fondations",
                Description = "Renforcez vos muscles profonds et améliorez votre posture.",
                CoachName = "Sophie Martin",
                CoachBio = "Professeure certifiée de Yoga Vinyasa avec 5 ans d'expérience.",
                Date = monday.AddDays(2),
                StartTime = "12:15",
                EndTime = "13:15",
                Capacity = 8
            },
            new Session
            {
                Id = Guid.NewGuid(),
                Title = "Méditation Guidée",
                Description = "Une pause relaxante pour relâcher la pression de la semaine.",
                CoachName = "Emma Leroy",
                CoachBio = "Praticienne en méditation de pleine conscience.",
                Date = monday.AddDays(4),
                StartTime = "19:00",
                EndTime = "20:00",
                Capacity = 15
            }
        };

        context.Sessions.AddRange(sessions);
        context.SaveChanges();
    }
}
