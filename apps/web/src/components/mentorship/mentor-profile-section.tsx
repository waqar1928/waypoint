"use client";

import { useState } from "react";
import { BecomeMentorForm } from "@/components/mentorship/become-mentor-form";
import { apiMutate } from "@/lib/api-client";
import type { MentorProfile } from "@/lib/mentorship";
import type { BecomeMentorInput } from "@/lib/validation";

export function MentorProfileSection({ initialProfile }: { initialProfile: MentorProfile | null }) {
  const [profile, setProfile] = useState(initialProfile);
  const [error, setError] = useState<string | null>(null);

  const handleSave = async (values: BecomeMentorInput) => {
    setError(null);
    const expertise = values.expertise
      .split(",")
      .map((e) => e.trim())
      .filter(Boolean);
    const yearsExperience = values.yearsExperience ? Number(values.yearsExperience) : null;

    const response = await apiMutate("/api/mentorship/mentors/me", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ expertise, yearsExperience, availability: values.availability || null }),
    });
    if (!response.ok) {
      setError("We couldn't save your mentor profile. Please try again.");
      return;
    }
    setProfile((await response.json()) as MentorProfile);
  };

  return (
    <div>
      {error ? (
        <p role="alert" className="mb-2 text-sm text-merlot-600">
          {error}
        </p>
      ) : null}
      <BecomeMentorForm initialProfile={profile} onSave={handleSave} />
    </div>
  );
}
