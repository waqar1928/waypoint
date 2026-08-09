import { getHelpRequests, getMentorDirectory, getMyMentorProfile } from "@/lib/mentorship";
import { MentorProfileSection } from "@/components/mentorship/mentor-profile-section";
import { MentorDirectory } from "@/components/mentorship/mentor-directory";
import { HelpRequestsBoard } from "@/components/mentorship/help-requests-board";

export default async function MentorshipPage() {
  const [myProfile, mentors, helpRequests] = await Promise.all([
    getMyMentorProfile(),
    getMentorDirectory(),
    getHelpRequests(),
  ]);

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Mentorship</h1>
      <p className="mt-2 text-ink-700">Ask for help, or opt in to give it.</p>

      <div className="mt-8 space-y-6">
        <MentorProfileSection initialProfile={myProfile} />
        <MentorDirectory initialMentors={mentors} />
        <div>
          <h2 className="font-display text-lg font-semibold text-ink-900">Help requests</h2>
          <p className="mt-1 text-sm text-ink-700">Ask a question, or answer someone else&rsquo;s.</p>
          <div className="mt-3">
            <HelpRequestsBoard initialRequests={helpRequests} />
          </div>
        </div>
      </div>
    </div>
  );
}
