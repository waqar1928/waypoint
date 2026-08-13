"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError } from "@/components/ui/field";
import { becomeMentorSchema, type BecomeMentorInput } from "@/lib/validation";
import type { MentorProfile } from "@/lib/mentorship";

export function BecomeMentorForm({
  initialProfile,
  onSave,
}: {
  initialProfile: MentorProfile | null;
  onSave: (values: BecomeMentorInput) => Promise<void>;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BecomeMentorInput>({
    resolver: zodResolver(becomeMentorSchema),
    defaultValues: {
      expertise: initialProfile?.expertise.join(", ") ?? "",
      yearsExperience: initialProfile?.yearsExperience?.toString() ?? "",
      availability: initialProfile?.availability ?? "",
    },
  });

  return (
    <Card>
      <h2 className="font-display text-lg font-semibold text-ink-900">
        {initialProfile ? "Your mentor profile" : "Become a mentor"}
      </h2>
      <p className="mt-1 text-sm text-ink-700">
        Opt in to answer help requests from other people building their dream. No verification
        required to start. That&rsquo;s a trust signal that builds over time, not a gate.
      </p>
      {initialProfile ? (
        <p className="mt-2 text-xs text-ink-500">
          Verification: <span className="font-medium text-ink-700">{initialProfile.verificationStatus}</span>
        </p>
      ) : null}

      <form
        className="mt-4 space-y-4"
        onSubmit={handleSubmit(async (values) => {
          await onSave(values);
        })}
        noValidate
      >
        <div>
          <Label htmlFor="mentor-expertise">Areas of expertise (comma-separated)</Label>
          <Input
            id="mentor-expertise"
            placeholder="marketing, operations, fundraising"
            aria-invalid={!!errors.expertise}
            {...register("expertise")}
          />
          <FieldError id="mentor-expertise-error">{errors.expertise?.message}</FieldError>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label htmlFor="mentor-years">Years of experience</Label>
            <Input id="mentor-years" type="number" min={0} {...register("yearsExperience")} />
          </div>
          <div>
            <Label htmlFor="mentor-availability">Availability</Label>
            <Input id="mentor-availability" placeholder="2 hours/week" {...register("availability")} />
          </div>
        </div>

        <Button type="submit" isLoading={isSubmitting}>
          {initialProfile ? "Save changes" : "Become a mentor"}
        </Button>
      </form>
    </Card>
  );
}
