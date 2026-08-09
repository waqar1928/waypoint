"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { MentorProfile } from "@/lib/mentorship";
import type { VerificationStatus } from "@/lib/admin";

const statusLabels: Record<VerificationStatus, string> = {
  unverified: "Unverified",
  pending: "Pending",
  verified: "Verified",
};

const statusClasses: Record<VerificationStatus, string> = {
  unverified: "bg-ink-300/40 text-ink-700",
  pending: "bg-beacon-500/10 text-beacon-600",
  verified: "bg-sage-100 text-sage-600",
};

export function AdminMentorsTable({ initialMentors }: { initialMentors: MentorProfile[] }) {
  const [mentors, setMentors] = useState(initialMentors);
  const [pendingId, setPendingId] = useState<string | null>(null);

  const setStatus = async (mentorProfileId: string, status: VerificationStatus) => {
    setPendingId(mentorProfileId);
    const response = await apiMutate(`/api/admin/mentors/${mentorProfileId}/verification`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ status }),
    });
    setPendingId(null);
    if (response.ok) {
      setMentors((prev) =>
        prev.map((m) => (m.id === mentorProfileId ? { ...m, verificationStatus: status } : m)),
      );
    }
  };

  if (mentors.length === 0) {
    return (
      <Card>
        <p className="text-sm text-ink-500">No mentor profiles yet.</p>
      </Card>
    );
  }

  return (
    <ul className="space-y-3">
      {mentors.map((mentor) => (
        <li key={mentor.id}>
          <Card>
            <div className="flex items-center justify-between gap-2">
              <p className="text-sm font-medium text-ink-900">{mentor.mentor.displayName}</p>
              <span className={`rounded-full px-2 py-0.5 text-[11px] font-medium ${statusClasses[mentor.verificationStatus]}`}>
                {statusLabels[mentor.verificationStatus]}
              </span>
            </div>
            <p className="mt-1 text-xs text-ink-500">
              {mentor.expertise.join(" · ")}
              {mentor.yearsExperience ? ` · ${mentor.yearsExperience} yrs` : ""}
              {mentor.availability ? ` · ${mentor.availability}` : ""}
            </p>

            <div className="mt-3 flex flex-wrap gap-2">
              {mentor.verificationStatus !== "verified" ? (
                <Button
                  variant="primary"
                  isLoading={pendingId === mentor.id}
                  onClick={() => setStatus(mentor.id, "verified")}
                >
                  Verify
                </Button>
              ) : null}
              {mentor.verificationStatus !== "pending" ? (
                <Button
                  variant="secondary"
                  isLoading={pendingId === mentor.id}
                  onClick={() => setStatus(mentor.id, "pending")}
                >
                  Mark pending
                </Button>
              ) : null}
              {mentor.verificationStatus !== "unverified" ? (
                <Button
                  variant="ghost"
                  isLoading={pendingId === mentor.id}
                  onClick={() => setStatus(mentor.id, "unverified")}
                >
                  Revoke
                </Button>
              ) : null}
            </div>
          </Card>
        </li>
      ))}
    </ul>
  );
}
