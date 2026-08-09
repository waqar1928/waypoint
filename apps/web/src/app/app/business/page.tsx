import Link from "next/link";
import { redirect } from "next/navigation";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";
import { getMyDream } from "@/lib/dream";
import { getMyBusinessIdea, getMyBusinessValidations } from "@/lib/business";
import { BusinessBuilderWorkspace } from "@/components/business/business-builder-workspace";

export default async function BusinessPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  if (!dream.isBusinessShaped) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
        <h1 className="font-display text-3xl font-semibold text-ink-900">Business Builder</h1>
        <p className="mt-2 text-ink-700">Build out and stress-test the business version of your dream.</p>
        <Card className="mt-8">
          <h2 className="font-display text-base font-semibold text-ink-900">
            This dream isn&rsquo;t marked as business-shaped yet
          </h2>
          <p className="mt-2 text-sm text-ink-700">
            Business Builder is for dreams that are meant to become a business. Mark &ldquo;{dream.title}&rdquo; as
            business-shaped on the Dream page to unlock it.
          </p>
          <Link href="/app/dream" className={buttonClasses("primary", "mt-4")}>
            Go to Dream
          </Link>
        </Card>
      </div>
    );
  }

  const [idea, validations] = await Promise.all([getMyBusinessIdea(), getMyBusinessValidations()]);

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Business Builder</h1>
      <p className="mt-2 text-ink-700">
        Build out and stress-test the business version of &ldquo;{dream.title}&rdquo;. Fill in what you know —
        everything here is optional and editable any time.
      </p>
      <div className="mt-8">
        <BusinessBuilderWorkspace initialIdea={idea} initialValidations={validations} />
      </div>
    </div>
  );
}
