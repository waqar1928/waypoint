import { getCommunityFeed } from "@/lib/community";
import { CommunityBoard } from "@/components/community/community-board";

export default async function CommunityPage() {
  const posts = await getCommunityFeed();

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Community</h1>
      <p className="mt-2 text-ink-700">
        An opt-in space to share progress and see how other people are turning their dreams into plans.
      </p>
      <div className="mt-8">
        <CommunityBoard initialPosts={posts} />
      </div>
    </div>
  );
}
